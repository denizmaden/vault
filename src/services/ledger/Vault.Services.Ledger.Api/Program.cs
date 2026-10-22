using Vault.BuildingBlocks.Api;
using Vault.BuildingBlocks.Api.Authentication;
using Vault.BuildingBlocks.Api.HealthChecks;
using Vault.Platform.Messaging;
using Vault.Platform.Observability;
using Vault.Services.Ledger.Application;
using Vault.Services.Ledger.Infrastructure;
using Vault.Services.Ledger.Infrastructure.Persistence;
using Vault.Services.Ledger.Infrastructure.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.AddVaultObservability("Vault.Ledger", "1.0.0");
builder.Services.AddVaultMessaging(builder.Configuration, "vault-ledger");
builder.Services.AddControllers();
builder.Services.AddBuildingBlocksApi();
builder.Services.AddVaultApiAuthentication(builder.Configuration, "vault.ledger.api", "vault.ledger");
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<LedgerDbContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddLedgerApplication()
    .AddLedgerInfrastructure(builder.Configuration);

var app = builder.Build();
await app.Services.MigrateLedgerDatabaseAsync();

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
    Service = "Ledger",
    Status = "Healthy"
})).AllowAnonymous();

app.Run();

public partial class Program;
