using System.Diagnostics;
using System.Text;
using Confluent.Kafka;
using Vault.Platform.Messaging.Abstractions;
using Vault.Platform.Messaging.Abstractions.Telemetry;

namespace Vault.Platform.Messaging.Kafka;

public sealed class KafkaMessagePublisher(IProducer<string, string> producer) : IMessagePublisher
{
    public async Task PublishAsync(MessageEnvelope envelope, CancellationToken cancellationToken = default)
    {
        using var activity = MessagingTelemetry.StartPublish(
            "kafka",
            envelope.Destination,
            envelope.Type,
            envelope.Key);

        var started = Stopwatch.GetTimestamp();
        var message = new Message<string, string>
        {
            Key = envelope.Key,
            Value = envelope.Payload,
            Headers = new Headers()
        };

        message.Headers.Add("message-type", Encoding.UTF8.GetBytes(envelope.Type));
        message.Headers.Add("occurred-on-utc", Encoding.UTF8.GetBytes(envelope.OccurredOnUtc.ToString("O")));

        var headers = MessagingTelemetry.CreatePropagationHeaders(envelope.Headers);
        foreach (var header in headers)
        {
            message.Headers.Add(header.Key, Encoding.UTF8.GetBytes(header.Value));
        }

        try
        {
            await producer.ProduceAsync(envelope.Destination, message, cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            MessagingTelemetry.RecordPublished(
                "kafka",
                envelope.Destination,
                envelope.Type,
                Stopwatch.GetElapsedTime(started));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
            MessagingTelemetry.RecordPublishFailure(
                "kafka",
                envelope.Destination,
                envelope.Type,
                Stopwatch.GetElapsedTime(started),
                exception.GetType().Name);
            throw;
        }
    }
}
