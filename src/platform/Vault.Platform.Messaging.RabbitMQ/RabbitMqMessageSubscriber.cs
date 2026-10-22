using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Vault.Platform.Messaging.Abstractions;
using Vault.Platform.Messaging.Abstractions.Telemetry;
using Vault.Platform.Messaging.RabbitMQ.Abstractions;

namespace Vault.Platform.Messaging.RabbitMQ;

internal sealed class RabbitMqMessageSubscriber(
    IRabbitMqConnectionProvider connectionProvider,
    ILogger<RabbitMqMessageSubscriber> logger) : IMessageSubscriber
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

        using var channel = connectionProvider.GetConnection().CreateModel();
        channel.BasicQos(0, 1, false);
        channel.QueueDeclare(destination, durable: true, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, eventArgs) =>
        {
            var envelope = ToEnvelope(destination, eventArgs);
            var traceParent = GetHeader(envelope.Headers, MessageTelemetryHeaders.TraceParent);
            var traceState = GetHeader(envelope.Headers, MessageTelemetryHeaders.TraceState);
            using var activity = MessagingTelemetry.StartConsume(
                "rabbitmq",
                destination,
                envelope.Type,
                envelope.Key,
                traceParent,
                traceState);
            using var scope = logger.BeginScope(new Dictionary<string, object>
            {
                ["messaging.system"] = "rabbitmq",
                ["messaging.destination.name"] = destination,
                ["messaging.message.type"] = envelope.Type,
                ["messaging.message.key"] = envelope.Key
            });
            var started = Stopwatch.GetTimestamp();

            try
            {
                await handler(envelope, cancellationToken);
                channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                activity?.SetStatus(ActivityStatusCode.Ok);
                MessagingTelemetry.RecordConsumed(
                    "rabbitmq",
                    destination,
                    envelope.Type,
                    Stopwatch.GetElapsedTime(started));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                if (channel.IsOpen)
                {
                    channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);
                }
            }
            catch (Exception exception)
            {
                activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
                MessagingTelemetry.RecordConsumeFailure(
                    "rabbitmq",
                    destination,
                    envelope.Type,
                    Stopwatch.GetElapsedTime(started),
                    exception.GetType().Name);
                logger.LogError(
                    exception,
                    "RabbitMQ message handling failed for destination {Destination} and message type {MessageType}.",
                    destination,
                    envelope.Type);

                if (channel.IsOpen)
                {
                    channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);
                }
            }
        };

        var consumerTag = channel.BasicConsume(
            queue: destination,
            autoAck: false,
            consumerTag: consumerGroup,
            noLocal: false,
            exclusive: false,
            arguments: null,
            consumer: consumer);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        finally
        {
            if (channel.IsOpen)
            {
                channel.BasicCancel(consumerTag);
            }
        }
    }

    private static MessageEnvelope ToEnvelope(string destination, BasicDeliverEventArgs eventArgs)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (eventArgs.BasicProperties.Headers is not null)
        {
            foreach (var header in eventArgs.BasicProperties.Headers)
            {
                headers[header.Key] = header.Value switch
                {
                    byte[] bytes => Encoding.UTF8.GetString(bytes),
                    string value => value,
                    _ => header.Value?.ToString() ?? string.Empty
                };
            }
        }

        var timestamp = eventArgs.BasicProperties.Timestamp.UnixTime;
        var occurredOnUtc = timestamp > 0
            ? DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime
            : DateTime.UtcNow;

        return new MessageEnvelope(
            destination,
            eventArgs.BasicProperties.MessageId ?? eventArgs.DeliveryTag.ToString(),
            eventArgs.BasicProperties.Type ?? GetHeader(headers, "message-type") ?? "unknown",
            Encoding.UTF8.GetString(eventArgs.Body.ToArray()),
            occurredOnUtc,
            headers);
    }

    private static string? GetHeader(IReadOnlyDictionary<string, string>? headers, string name) =>
        headers is not null && headers.TryGetValue(name, out var value) ? value : null;
}
