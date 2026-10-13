namespace Vault.Platform.Observability.Configuration;

public sealed class VaultObservabilityOptions
{
    public const string SectionName = "Observability";

    public VaultLoggingOptions Logging { get; init; } = new();

    public VaultTelemetrySignalOptions Tracing { get; init; } = new();

    public VaultTelemetrySignalOptions Metrics { get; init; } = new();
}

public sealed class VaultLoggingOptions
{
    public bool Enabled { get; init; } = true;

    public string Provider { get; init; } = "Serilog";
}

public sealed class VaultTelemetrySignalOptions
{
    public bool Enabled { get; init; } = true;
}
