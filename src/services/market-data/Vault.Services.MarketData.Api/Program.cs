using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Vault.BuildingBlocks.Api;
using Vault.BuildingBlocks.Api.Authentication;
using Vault.BuildingBlocks.Api.HealthChecks;
using Vault.Platform.Messaging;
using Vault.Platform.Observability;
using Vault.Services.MarketData.Api.Security;
using Vault.Services.MarketData.Api.WebSockets;
using Vault.Services.MarketData.Application;
using Vault.Services.MarketData.Application.Abstractions;
using Vault.Services.MarketData.Application.Mapping;
using Vault.Services.MarketData.Infrastructure;

const string RestRateLimitPolicy = "market-data-rest";
const string WebSocketRateLimitPolicy = "market-data-websocket";

var builder = WebApplication.CreateBuilder(args);

var webSocketSettings = builder.Configuration
    .GetSection(MarketDataWebSocketSettings.SectionName)
    .Get<MarketDataWebSocketSettings>()
    ?? new MarketDataWebSocketSettings();
var securitySettings = builder.Configuration
    .GetSection(MarketDataSecuritySettings.SectionName)
    .Get<MarketDataSecuritySettings>()
    ?? new MarketDataSecuritySettings();

if (webSocketSettings.MaxConnections <= 0
    || webSocketSettings.MaxConnectionsPerClient <= 0
    || webSocketSettings.OutboundQueueCapacity <= 0
    || webSocketSettings.MaxIncomingMessageBytes < 1024
    || webSocketSettings.CommandsPerMinute <= 0)
{
    throw new InvalidOperationException("MarketData WebSocket settings are invalid.");
}

if (securitySettings.RestRequestsPerMinute <= 0
    || securitySettings.WebSocketHandshakesPerMinute <= 0)
{
    throw new InvalidOperationException("MarketData security quota settings are invalid.");
}

builder.AddVaultObservability("Vault.MarketData", "1.0.0");
builder.Services.AddVaultMessaging(builder.Configuration, "vault-market-data");
builder.Services.AddBuildingBlocksApi();
builder.Services.AddVaultApiAuthentication(
    builder.Configuration,
    "vault.marketdata.api",
    "vault.marketdata");
builder.Services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Events ??= new JwtBearerEvents();
    options.Events.OnMessageReceived = context =>
    {
        if (context.Request.Path.StartsWithSegments("/ws/market-prices")
            && context.Request.Query.TryGetValue("access_token", out var accessToken)
            && !string.IsNullOrWhiteSpace(accessToken))
        {
            context.Token = accessToken;
        }

        return Task.CompletedTask;
    };
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/problem+json";
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            Type = "https://httpstatuses.com/429",
            Title = "Request quota exceeded.",
            Status = StatusCodes.Status429TooManyRequests,
            Detail = "Too many requests were sent by this client."
        }, cancellationToken);
    };
    options.AddPolicy(RestRateLimitPolicy, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetClientPartitionKey(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = securitySettings.RestRequestsPerMinute,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    options.AddPolicy(WebSocketRateLimitPolicy, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetClientPartitionKey(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = securitySettings.WebSocketHandshakesPerMinute,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(webSocketSettings);
builder.Services.AddSingleton(securitySettings);
builder.Services
    .AddMarketDataApplication()
    .AddMarketDataInfrastructure(builder.Configuration);
builder.Services.AddSingleton<MarketPriceWebSocketHub>();
builder.Services.AddSingleton<IMarketPriceClientNotifier>(serviceProvider =>
    serviceProvider.GetRequiredService<MarketPriceWebSocketHub>());

var app = builder.Build();

app.UseVaultObservability();
app.UseBuildingBlocksApi();
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});
app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(20)
});
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    Service = "MarketData",
    Status = "Healthy",
    Dashboard = "/market",
    WebSocket = "/ws/market-prices"
}));

app.MapGet("/market", async context =>
{
    var webRootPath = app.Environment.WebRootPath
        ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
    var indexPath = Path.Combine(webRootPath, "index.html");

    if (!File.Exists(indexPath))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsync(
            "MarketData dashboard has not been built. Run npm run build in ClientApp.",
            context.RequestAborted);
        return;
    }

    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(indexPath);
});

var marketPrices = app.MapGroup("/api/market-prices")
    .RequireRateLimiting(RestRateLimitPolicy);

marketPrices.MapGet("/instruments", (IMarketInstrumentCatalog catalog) =>
    Results.Ok(catalog.Instruments
        .OrderBy(instrument => instrument.Kind)
        .ThenBy(instrument => instrument.Symbol)
        .Select(instrument => new
        {
            instrument.Symbol,
            instrument.DisplayName,
            instrument.BaseAsset,
            instrument.QuoteAsset,
            Kind = instrument.Kind.ToString()
        })));

marketPrices.MapGet("/", async (
    string? symbols,
    IMarketPriceSnapshotStore snapshotStore,
    CancellationToken cancellationToken) =>
{
    var requestedSymbols = string.IsNullOrWhiteSpace(symbols)
        ? null
        : symbols.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    var prices = await snapshotStore.GetSnapshotAsync(requestedSymbols, cancellationToken);
    return Results.Ok(new
    {
        SentOnUtc = DateTime.UtcNow,
        Prices = prices.Select(price => price.ToDto()).ToArray()
    });
});

marketPrices.MapGet("/{symbol}", async (
    string symbol,
    IMarketPriceSnapshotStore snapshotStore,
    CancellationToken cancellationToken) =>
{
    var price = await snapshotStore.GetBySymbolAsync(symbol, cancellationToken);
    return price is null
        ? Results.NotFound()
        : Results.Ok(price.ToDto());
});

var webSocketEndpoint = app.Map("/ws/market-prices", async context =>
{
    var hub = context.RequestServices.GetRequiredService<MarketPriceWebSocketHub>();
    await hub.HandleAsync(context, context.RequestAborted);
}).RequireRateLimiting(WebSocketRateLimitPolicy);

if (securitySettings.RequireAuthentication)
{
    marketPrices.RequireAuthorization(VaultApiAuthenticationDefaults.ScopePolicyName);
    webSocketEndpoint.RequireAuthorization(VaultApiAuthenticationDefaults.ScopePolicyName);
}

app.MapVaultHealthChecks();

app.Run();

static string GetClientPartitionKey(HttpContext context) =>
    context.User.FindFirstValue("sub")
    ?? context.Connection.RemoteIpAddress?.ToString()
    ?? "unknown";

public partial class Program;
