using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Vault.Platform.Observability.Tracing.OpenTelemetry;

public static class DependencyInjection
{
    public static IServiceCollection AddVaultOpenTelemetryTracing(
        this IServiceCollection services,
        string serviceName,
        string serviceVersion,
        string environmentName,
        string? otlpEndpoint,
        double samplingRatio)
    {
        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
                            .AddAttributes(new Dictionary<string, object>
                            {
                                ["deployment.environment"] = environmentName,
                                ["service.instance.id"] = Environment.MachineName
                            }))
                    .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(samplingRatio)))
                    .AddSource(serviceName)
                    .AddSource("Vault.BuildingBlocks.Application")
                    .AddSource("Vault.Platform.Messaging")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation();

                if (Uri.TryCreate(otlpEndpoint, UriKind.Absolute, out var endpoint))
                {
                    tracing.AddOtlpExporter(options => options.Endpoint = endpoint);
                }
            });

        return services;
    }
}
