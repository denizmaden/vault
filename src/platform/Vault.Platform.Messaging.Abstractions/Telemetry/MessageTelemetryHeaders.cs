namespace Vault.Platform.Messaging.Abstractions.Telemetry;

public static class MessageTelemetryHeaders
{
    public const string TraceParent = "traceparent";
    public const string TraceState = "tracestate";
    public const string CorrelationId = "x-correlation-id";
}

