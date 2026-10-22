using Vault.BuildingBlocks.Api;
using Vault.BuildingBlocks.Api.Authentication;
using Vault.BuildingBlocks.Api.HealthChecks;
using Vault.Platform.Caching;
using Vault.Platform.Messaging;
using Vault.Platform.Observability;
using Vault.Services.Wallet.Application;
using Vault.Services.Wallet.Infrastructure;
using Vault.Services.Wallet.Infrastructure.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
builder.AddVaultObservability("Vault.Wallet", "1.0.0");
builder.Services.AddVaultMessaging(builder.Configuration, "vault-wallet");
builder.Services.AddVaultCaching(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddBuildingBlocksApi();
builder.Services.AddVaultApiAuthentication(builder.Configuration, "vault.wallet.api", "vault.wallet");
builder.Services
    .AddHealthChecks()
    .AddCheck<MongoWalletHealthCheck>("wallet-mongo", tags: ["ready"]);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddWalletApplication()
    .AddWalletInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseVaultObservability();
app.UseBuildingBlocksApi();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireAuthorization(VaultApiAuthenticationDefaults.ScopePolicyName);
app.MapVaultHealthChecks();
app.MapGet("/", () => Results.Ok(new
{
    Service = "Wallet",
    Status = "Healthy"
})).AllowAnonymous();

app.Run();

public partial class Program;
