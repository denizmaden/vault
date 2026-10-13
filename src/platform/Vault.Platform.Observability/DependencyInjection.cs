using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vault.Platform.Observability.Configuration;
using Vault.Platform.Observability.Logging.NLog;
using Vault.Platform.Observability.Logging.Serilog;
using Vault.Platform.Observability.Metrics.OpenTelemetry;
using Vault.Platform.Observability.Tracing.OpenTelemetry;

namespace Vault.Platform.Observability;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddVaultObservability(
        this WebApplicationBuilder builder,
        string serviceName,
        string serviceVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceVersion);

        var options = builder.Configuration
            .GetSection(VaultObservabilityOptions.SectionName)
            .Get<VaultObservabilityOptions>()
            ?? new VaultObservabilityOptions();

        AddLogging(builder, serviceName, options.Logging);

        if (options.Tracing.Enabled)
        {
            builder.Services.AddVaultOpenTelemetryTracing(
                serviceName,
                serviceVersion,
                builder.Environment.EnvironmentName,
                ObservabilityEnvironment.GetOtlpEndpoint("traces"),
                ObservabilityEnvironment.GetTraceSamplingRatio());
        }

        if (options.Metrics.Enabled)
        {
            builder.Services.AddVaultOpenTelemetryMetrics(
                serviceName,
                serviceVersion,
                builder.Environment.EnvironmentName,
                ObservabilityEnvironment.GetOtlpEndpoint("metrics"));
        }

        return builder;
    }

    private static void AddLogging(
        WebApplicationBuilder builder,
        string serviceName,
        VaultLoggingOptions options)
    {
        if (!options.Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(options.Provider))
        {
            throw new InvalidOperationException(
                "Observability logging provider is missing. Supported providers: Serilog, NLog.");
        }

        builder.Logging.ClearProviders();

        switch (options.Provider.Trim().ToUpperInvariant())
        {
            case "SERILOG":
                builder.Host.AddVaultSerilogLogging(serviceName);
                break;

            case "NLOG":
                builder.Host.AddVaultNLogLogging(serviceName);
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported observability logging provider '{options.Provider}'. Supported providers: Serilog, NLog.");
        }
    }
}
