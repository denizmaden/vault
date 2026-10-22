using Vault.BuildingBlocks.Api;
using Vault.BuildingBlocks.Api.Authentication;
using Vault.BuildingBlocks.Api.HealthChecks;
using Vault.Platform.Messaging;
using Vault.Platform.Observability;
using Vault.Services.Transaction.Application;
using Vault.Services.Transaction.Infrastructure;
using Vault.Services.Transaction.Infrastructure.Persistence;
using Vault.Services.Transaction.Infrastructure.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.AddVaultObservability("Vault.Transaction", "1.0.0");
builder.Services.AddVaultMessaging(builder.Configuration, "vault-transaction");
builder.Services.AddControllers();
builder.Services.AddBuildingBlocksApi();
builder.Services.AddVaultApiAuthentication(builder.Configuration, "vault.transaction.api", "vault.transaction");
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<TransactionDbContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddTransactionApplication()
    .AddTransactionInfrastructure(builder.Configuration);

var app = builder.Build();
await app.Services.MigrateTransactionDatabaseAsync();

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
    Service = "Transaction",
    Status = "Healthy"
})).AllowAnonymous();

app.Run();

public partial class Program;
