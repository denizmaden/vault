using System.Diagnostics;
using System.Globalization;
using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Vault.Platform.Messaging.Abstractions;
using Vault.Platform.Messaging.Abstractions.Telemetry;
using Vault.Platform.Messaging.Kafka.Abstractions;

namespace Vault.Platform.Messaging.Kafka;

internal sealed class KafkaMessageSubscriber(
    IKafkaConsumerFactory consumerFactory,
    ILogger<KafkaMessageSubscriber> logger) : IMessageSubscriber
{
    public async Task SubscribeAsync(
        string destination,
        string consumerGroup,
        Func<MessageEnvelope, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerGroup);
        ArgumentNullException.ThrowIfNull(handler);

        using var consumer = consumerFactory.Create(consumerGroup);
        consumer.Subscribe(destination);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ConsumeResult<string, string>? consumeResult;

                try
                {
                    consumeResult = consumer.Consume(cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (ConsumeException exception)
                {
                    logger.LogError(exception, "Kafka consume failed for destination {Destination}.", destination);
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                    continue;
                }

                if (consumeResult?.Message is null)
                {
                    continue;
                }

                var envelope = ToEnvelope(destination, consumeResult.Message);
                var traceParent = GetHeader(envelope.Headers, MessageTelemetryHeaders.TraceParent);
                var traceState = GetHeader(envelope.Headers, MessageTelemetryHeaders.TraceState);

                using var activity = MessagingTelemetry.StartConsume(
                    "kafka",
                    destination,
                    envelope.Type,
                    envelope.Key,
                    traceParent,
                    traceState);
                using var scope = logger.BeginScope(new Dictionary<string, object>
                {
                    ["messaging.system"] = "kafka",
                    ["messaging.destination.name"] = destination,
                    ["messaging.message.type"] = envelope.Type,
                    ["messaging.message.key"] = envelope.Key
                });

                var started = Stopwatch.GetTimestamp();

                try
                {
                    await handler(envelope, cancellationToken);
                    consumer.Commit(consumeResult);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    MessagingTelemetry.RecordConsumed(
                        "kafka",
                        destination,
                        envelope.Type,
                        Stopwatch.GetElapsedTime(started));
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
                    MessagingTelemetry.RecordConsumeFailure(
                        "kafka",
                        destination,
                        envelope.Type,
                        Stopwatch.GetElapsedTime(started),
                        exception.GetType().Name);
                    logger.LogError(
                        exception,
                        "Kafka message handling failed for destination {Destination} and message type {MessageType}.",
                        destination,
                        envelope.Type);
                }
            }
        }
        finally
        {
            consumer.Close();
        }
    }

    private static MessageEnvelope ToEnvelope(string destination, Message<string, string> message)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in message.Headers)
        {
            headers[header.Key] = Encoding.UTF8.GetString(header.GetValueBytes());
        }

        var occurredOnUtc = DateTime.TryParse(
            GetHeader(headers, "occurred-on-utc"),
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out var parsed)
            ? parsed.ToUniversalTime()
            : DateTime.UtcNow;

        return new MessageEnvelope(
            destination,
            message.Key ?? string.Empty,
            GetHeader(headers, "message-type") ?? "unknown",
            message.Value ?? string.Empty,
            occurredOnUtc,
            headers);
    }

    private static string? GetHeader(IReadOnlyDictionary<string, string>? headers, string name)
    {
        return headers is not null && headers.TryGetValue(name, out var value)
            ? value
            : null;
    }
}
