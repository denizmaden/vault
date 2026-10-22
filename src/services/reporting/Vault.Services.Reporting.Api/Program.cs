using Vault.BuildingBlocks.Api;
using Vault.BuildingBlocks.Api.Authentication;
using Vault.BuildingBlocks.Api.HealthChecks;
using Vault.Platform.Messaging;
using Vault.Platform.Observability;
using Vault.Services.Reporting.Application;
using Vault.Services.Reporting.Infrastructure;
using Vault.Services.Reporting.Infrastructure.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
builder.AddVaultObservability("Vault.Reporting", "1.0.0");
builder.Services.AddVaultMessaging(builder.Configuration, "vault-reporting");
builder.Services.AddControllers();
builder.Services.AddBuildingBlocksApi();
builder.Services.AddVaultApiAuthentication(builder.Configuration, "vault.reporting.api", "vault.reporting");
builder.Services
    .AddHealthChecks()
    .AddCheck<MongoReportingHealthCheck>("reporting-mongo", tags: ["ready"]);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddReportingApplication()
    .AddReportingInfrastructure(builder.Configuration);

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
    Service = "Reporting",
    Status = "Healthy"
})).AllowAnonymous();

app.Run();

public partial class Program;
