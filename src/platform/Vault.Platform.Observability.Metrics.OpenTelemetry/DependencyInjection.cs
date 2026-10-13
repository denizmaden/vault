using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace Vault.Platform.Observability.Metrics.OpenTelemetry;

public static class DependencyInjection
{
    public static IServiceCollection AddVaultOpenTelemetryMetrics(
        this IServiceCollection services,
        string serviceName,
        string serviceVersion,
        string environmentName,
        string? otlpEndpoint)
    {
        services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
                            .AddAttributes(new Dictionary<string, object>
                            {
                                ["deployment.environment"] = environmentName,
                                ["service.instance.id"] = Environment.MachineName
                            }))
                    .AddMeter(serviceName)
                    .AddMeter("Vault.BuildingBlocks.Application")
                    .AddMeter("Vault.Platform.Messaging")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (Uri.TryCreate(otlpEndpoint, UriKind.Absolute, out var endpoint))
                {
                    metrics.AddOtlpExporter(options => options.Endpoint = endpoint);
                }
            });

        return services;
    }
}
