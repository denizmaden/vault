using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Vault.BuildingBlocks.Application.Telemetry;

internal static class ApplicationTelemetry
{
    public const string ActivitySourceName = "Vault.BuildingBlocks.Application";
    public const string MeterName = "Vault.BuildingBlocks.Application";

    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    private static readonly Meter Meter = new(MeterName);

    private static readonly Counter<long> RequestsStarted = Meter.CreateCounter<long>(
        "vault.application.requests.started",
        unit: "{request}",
        description: "Number of application commands and queries started.");

    private static readonly Counter<long> RequestsCompleted = Meter.CreateCounter<long>(
        "vault.application.requests.completed",
        unit: "{request}",
        description: "Number of application commands and queries completed.");

    private static readonly Counter<long> RequestsFailed = Meter.CreateCounter<long>(
        "vault.application.requests.failed",
        unit: "{request}",
        description: "Number of application commands and queries failed.");

    private static readonly Counter<long> ValidationFailures = Meter.CreateCounter<long>(
        "vault.application.validation.failures",
        unit: "{failure}",
        description: "Number of application validation failures.");

    private static readonly Histogram<double> RequestDuration = Meter.CreateHistogram<double>(
        "vault.application.request.duration",
        unit: "ms",
        description: "Application command and query execution duration.");

    public static Activity? StartRequest(string requestKind, string requestName)
    {
        var activity = ActivitySource.StartActivity($"{requestKind} {requestName}", ActivityKind.Internal);
        activity?.SetTag("vault.request.kind", requestKind);
        activity?.SetTag("vault.request.name", requestName);

        RequestsStarted.Add(1, Tags(requestKind, requestName));

        return activity;
    }

    public static void RecordSuccess(string requestKind, string requestName, TimeSpan duration)
    {
        var tags = Tags(requestKind, requestName, outcome: "success");
        RequestsCompleted.Add(1, tags);
        RequestDuration.Record(duration.TotalMilliseconds, tags);
    }

    public static void RecordFailure(string requestKind, string requestName, TimeSpan duration, string failureType)
    {
        var tags = Tags(requestKind, requestName, outcome: "failure", failureType: failureType);
        RequestsFailed.Add(1, tags);
        RequestDuration.Record(duration.TotalMilliseconds, tags);
    }

    public static void RecordValidationFailure(string requestKind, string requestName, int errorCount)
    {
        ValidationFailures.Add(errorCount, Tags(requestKind, requestName, outcome: "validation_failure"));
    }

    private static KeyValuePair<string, object?>[] Tags(
        string requestKind,
        string requestName,
        string? outcome = null,
        string? failureType = null)
    {
        var tags = new List<KeyValuePair<string, object?>>
        {
            new("vault.request.kind", requestKind),
            new("vault.request.name", requestName)
        };

        if (!string.IsNullOrWhiteSpace(outcome))
        {
            tags.Add(new KeyValuePair<string, object?>("vault.request.outcome", outcome));
        }

        if (!string.IsNullOrWhiteSpace(failureType))
        {
            tags.Add(new KeyValuePair<string, object?>("vault.failure.type", failureType));
        }

        return tags.ToArray();
    }
}
