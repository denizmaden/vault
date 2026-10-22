using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Vault.Platform.Messaging.Abstractions.Telemetry;

public static class MessagingTelemetry
{
    public const string ActivitySourceName = "Vault.Platform.Messaging";
    public const string MeterName = "Vault.Platform.Messaging";

    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    private static readonly Meter Meter = new(MeterName);

    private static readonly Counter<long> MessagesPublished = Meter.CreateCounter<long>(
        "vault.messaging.messages.published",
        unit: "{message}",
        description: "Number of broker messages published.");

    private static readonly Counter<long> PublishFailures = Meter.CreateCounter<long>(
        "vault.messaging.publish.failures",
        unit: "{failure}",
        description: "Number of broker publish failures.");

    private static readonly Histogram<double> PublishDuration = Meter.CreateHistogram<double>(
        "vault.messaging.publish.duration",
        unit: "ms",
        description: "Broker publish duration.");

    private static readonly Counter<long> MessagesConsumed = Meter.CreateCounter<long>(
        "vault.messaging.messages.consumed",
        unit: "{message}",
        description: "Number of broker messages consumed.");

    private static readonly Counter<long> ConsumeFailures = Meter.CreateCounter<long>(
        "vault.messaging.consume.failures",
        unit: "{failure}",
        description: "Number of broker consume failures.");

    private static readonly Histogram<double> ConsumeDuration = Meter.CreateHistogram<double>(
        "vault.messaging.consume.duration",
        unit: "ms",
        description: "Broker consume handling duration.");

    private static readonly Counter<long> OutboxMessagesDispatched = Meter.CreateCounter<long>(
        "vault.outbox.messages.dispatched",
        unit: "{message}",
        description: "Number of outbox messages dispatched.");

    private static readonly Counter<long> OutboxDispatchFailures = Meter.CreateCounter<long>(
        "vault.outbox.dispatch.failures",
        unit: "{failure}",
        description: "Number of outbox dispatch failures.");

    public static Activity? StartPublish(
        string messagingSystem,
        string destination,
        string messageType,
        string messageKey)
    {
        var activity = ActivitySource.StartActivity($"{messagingSystem} publish {destination}", ActivityKind.Producer);
        Enrich(activity, messagingSystem, destination, "publish", messageType, messageKey);
        return activity;
    }

    public static Activity? StartConsume(
        string messagingSystem,
        string destination,
        string messageType,
        string messageKey,
        string? traceParent,
        string? traceState)
    {
        Activity? activity;
        if (ActivityContext.TryParse(traceParent, traceState, out var parentContext))
        {
            activity = ActivitySource.StartActivity($"{messagingSystem} consume {destination}", ActivityKind.Consumer, parentContext);
        }
        else
        {
            activity = ActivitySource.StartActivity($"{messagingSystem} consume {destination}", ActivityKind.Consumer);
        }

        Enrich(activity, messagingSystem, destination, "consume", messageType, messageKey);
        return activity;
    }

    public static IReadOnlyDictionary<string, string> CreatePropagationHeaders(IReadOnlyDictionary<string, string>? headers)
    {
        var propagatedHeaders = headers is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(headers, StringComparer.OrdinalIgnoreCase);

        var activity = Activity.Current;
        if (activity is null)
        {
            return propagatedHeaders;
        }

        if (!string.IsNullOrWhiteSpace(activity.Id))
        {
            propagatedHeaders[MessageTelemetryHeaders.TraceParent] = activity.Id;
        }

        if (!string.IsNullOrWhiteSpace(activity.TraceStateString))
        {
            propagatedHeaders[MessageTelemetryHeaders.TraceState] = activity.TraceStateString;
        }

        return propagatedHeaders;
    }

    public static void RecordPublished(string messagingSystem, string destination, string messageType, TimeSpan duration)
    {
        var tags = Tags(messagingSystem, destination, "publish", messageType, outcome: "success");
        MessagesPublished.Add(1, tags);
        PublishDuration.Record(duration.TotalMilliseconds, tags);
    }

    public static void RecordPublishFailure(
        string messagingSystem,
        string destination,
        string messageType,
        TimeSpan duration,
        string failureType)
    {
        var tags = Tags(messagingSystem, destination, "publish", messageType, outcome: "failure", failureType);
        PublishFailures.Add(1, tags);
        PublishDuration.Record(duration.TotalMilliseconds, tags);
    }

    public static void RecordConsumed(string messagingSystem, string destination, string messageType, TimeSpan duration)
    {
        var tags = Tags(messagingSystem, destination, "consume", messageType, outcome: "success");
        MessagesConsumed.Add(1, tags);
        ConsumeDuration.Record(duration.TotalMilliseconds, tags);
    }

    public static void RecordConsumeFailure(
        string messagingSystem,
        string destination,
        string messageType,
        TimeSpan duration,
        string failureType)
    {
        var tags = Tags(messagingSystem, destination, "consume", messageType, outcome: "failure", failureType);
        ConsumeFailures.Add(1, tags);
        ConsumeDuration.Record(duration.TotalMilliseconds, tags);
    }

    public static void RecordOutboxDispatched(string serviceName, string destination, string messageType, long count)
    {
        OutboxMessagesDispatched.Add(count, OutboxTags(serviceName, destination, messageType, "success"));
    }

    public static void RecordOutboxFailure(string serviceName, string destination, string messageType, string failureType)
    {
        OutboxDispatchFailures.Add(1, OutboxTags(serviceName, destination, messageType, "failure", failureType));
    }

    private static void Enrich(
        Activity? activity,
        string messagingSystem,
        string destination,
        string operation,
        string messageType,
        string messageKey)
    {
        activity?.SetTag("messaging.system", messagingSystem);
        activity?.SetTag("messaging.destination.name", destination);
        activity?.SetTag("messaging.operation", operation);
        activity?.SetTag("messaging.message.type", messageType);
        activity?.SetTag("messaging.message.key", messageKey);
    }

    private static KeyValuePair<string, object?>[] Tags(
        string messagingSystem,
        string destination,
        string operation,
        string messageType,
        string? outcome = null,
        string? failureType = null)
    {
        var tags = new List<KeyValuePair<string, object?>>
        {
            new("messaging.system", messagingSystem),
            new("messaging.destination.name", destination),
            new("messaging.operation", operation),
            new("messaging.message.type", messageType)
        };

        if (!string.IsNullOrWhiteSpace(outcome))
        {
            tags.Add(new KeyValuePair<string, object?>("messaging.operation.outcome", outcome));
        }

        if (!string.IsNullOrWhiteSpace(failureType))
        {
            tags.Add(new KeyValuePair<string, object?>("vault.failure.type", failureType));
        }

        return tags.ToArray();
    }

    private static KeyValuePair<string, object?>[] OutboxTags(
        string serviceName,
        string destination,
        string messageType,
        string outcome,
        string? failureType = null)
    {
        var tags = new List<KeyValuePair<string, object?>>
        {
            new("service.name", serviceName),
            new("messaging.destination.name", destination),
            new("messaging.message.type", messageType),
            new("vault.outbox.outcome", outcome)
        };

        if (!string.IsNullOrWhiteSpace(failureType))
        {
            tags.Add(new KeyValuePair<string, object?>("vault.failure.type", failureType));
        }

        return tags.ToArray();
    }
}

