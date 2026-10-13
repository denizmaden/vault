using Vault.BuildingBlocks.Api.Authentication;
using Vault.BuildingBlocks.Api;
using Vault.BuildingBlocks.Api.HealthChecks;
using Vault.Platform.Observability;
using Vault.Services.Customer.Application;
using Vault.Services.Customer.Infrastructure;
using Vault.Services.Customer.Infrastructure.Persistence;
using Vault.Services.Customer.Infrastructure.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.AddVaultObservability("Vault.Customer", "1.0.0");
builder.Services.AddControllers();
builder.Services.AddBuildingBlocksApi();
builder.Services.AddVaultApiAuthentication(builder.Configuration, "vault.customer.api", "vault.customer");
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<CustomerDbContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddCustomerApplication()
    .AddCustomerInfrastructure(builder.Configuration);

var app = builder.Build();
await app.Services.MigrateCustomerDatabaseAsync();

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
    Service = "Customer",
    Status = "Healthy"
})).AllowAnonymous();

app.Run();

public partial class Program;
