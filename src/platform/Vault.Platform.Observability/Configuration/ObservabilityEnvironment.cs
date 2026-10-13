using System.Globalization;

namespace Vault.Platform.Observability.Configuration;

public static class ObservabilityEnvironment
{
    public static string? GetOtlpEndpoint(string signalName)
    {
        var signalSpecific = Environment.GetEnvironmentVariable($"OTEL_EXPORTER_OTLP_{signalName.ToUpperInvariant()}_ENDPOINT");
        if (!string.IsNullOrWhiteSpace(signalSpecific))
        {
            return signalSpecific;
        }

        return Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
    }

    public static double GetTraceSamplingRatio()
    {
        var configuredValue = Environment.GetEnvironmentVariable("VAULT_OTEL_TRACE_SAMPLING_RATIO")
            ?? Environment.GetEnvironmentVariable("OTEL_TRACES_SAMPLER_ARG");

        if (double.TryParse(configuredValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var ratio))
        {
            return Math.Clamp(ratio, 0.0, 1.0);
        }

        return 1.0;
    }
}
