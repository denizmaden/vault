using Duende.IdentityServer;
using Duende.IdentityServer.AspNetIdentity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Vault.BuildingBlocks.Api;
using Vault.BuildingBlocks.Api.HealthChecks;
using Vault.Services.Identity.Api.Configuration;
using Vault.Services.Identity.Api.IdentityServer;
using Vault.Services.Identity.Application;
using Vault.Services.Identity.Infrastructure;
using Vault.Services.Identity.Infrastructure.Identity;
using Vault.Services.Identity.Infrastructure.Persistence;
using Vault.Services.Identity.Infrastructure.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<IdentityServerSettings>()
    .Bind(builder.Configuration.GetSection(IdentityServerSettings.SectionName))
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<IdentityServerSettings>, IdentityServerSettingsValidator>();

var identityServerSettings = builder.Configuration
    .GetSection(IdentityServerSettings.SectionName)
    .Get<IdentityServerSettings>()
    ?? throw new InvalidOperationException("IdentityServer configuration is missing.");

builder.Services.AddControllers();
builder.Services.AddBuildingBlocksApi();
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<IdentityDbContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Vault Identity API",
        Version = "v1"
    });

    var oauthScheme = new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Password = new OpenApiOAuthFlow
            {
                TokenUrl = new Uri("/connect/token", UriKind.Relative),
                Scopes = new Dictionary<string, string>
                {
                    [IdentityServerConstants.LocalApi.ScopeName] = "Access the local identity API",
                    ["vault.identity.api"] = "Access the vault identity API"
                }
            }
        }
    };

    options.AddSecurityDefinition("oauth2", oauthScheme);
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste a valid access token."
    });
    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("oauth2", null, null),
            [
                IdentityServerConstants.LocalApi.ScopeName,
                "vault.identity.api"
            ]
        },
        {
            new OpenApiSecuritySchemeReference("Bearer", null, null),
            []
        }
    });
});

builder.Services
    .AddIdentityApplication()
    .AddIdentityInfrastructure(builder.Configuration);
builder.Services
    .AddIdentityServer(options =>
    {
        options.EmitStaticAudienceClaim = true;
        if (!string.IsNullOrWhiteSpace(identityServerSettings.IssuerUri))
        {
            options.IssuerUri = identityServerSettings.IssuerUri;
        }
    })
    .AddDeveloperSigningCredential()
    .AddInMemoryIdentityResources(IdentityServerConfiguration.IdentityResources)
    .AddInMemoryApiResources(IdentityServerConfiguration.ApiResources)
    .AddInMemoryApiScopes(IdentityServerConfiguration.ApiScopes)
    .AddInMemoryClients(IdentityServerConfiguration.CreateClients(identityServerSettings))
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = databaseBuilder =>
            databaseBuilder.UseSqlServer(
                builder.Configuration.GetConnectionString("IdentityDatabase"),
                sql => sql.MigrationsAssembly(typeof(IdentityDbContext).Assembly.GetName().Name));
        options.EnableTokenCleanup = true;
        options.TokenCleanupInterval = 3600;
    })
    .AddAspNetIdentity<VaultIdentityUser>();
builder.Services
    .AddAuthentication()
    .AddLocalApi();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        IdentityServerConstants.LocalApi.PolicyName,
        policy =>
        {
            policy.AddAuthenticationSchemes(IdentityServerConstants.LocalApi.AuthenticationScheme);
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("scope", IdentityServerConstants.LocalApi.ScopeName);
        });
});

var app = builder.Build();
await app.Services.MigrateIdentityDatabaseAsync();

app.UseBuildingBlocksApi();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Vault Identity API v1");
    options.OAuthClientId(identityServerSettings.SwaggerClientId);
    options.OAuthClientSecret(identityServerSettings.SwaggerClientSecret);
});
app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapVaultHealthChecks();
app.MapGet("/", () => Results.Ok(new
{
    Service = "Identity",
    Status = "Healthy"
}));

app.Run();

public partial class Program;
