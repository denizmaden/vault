using System.Diagnostics;
using System.Text;
using RabbitMQ.Client;
using Vault.Platform.Messaging.Abstractions;
using Vault.Platform.Messaging.Abstractions.Telemetry;
using Vault.Platform.Messaging.RabbitMQ.Abstractions;

namespace Vault.Platform.Messaging.RabbitMQ;

public sealed class RabbitMqMessagePublisher(IRabbitMqConnectionProvider connectionProvider) : IMessagePublisher
{
    public Task PublishAsync(MessageEnvelope envelope, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var activity = MessagingTelemetry.StartPublish(
            "rabbitmq",
            envelope.Destination,
            envelope.Type,
            envelope.Key);

        var started = Stopwatch.GetTimestamp();

        try
        {
            using var channel = connectionProvider.GetConnection().CreateModel();
            channel.ConfirmSelect();
            channel.QueueDeclare(
                queue: envelope.Destination,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.Type = envelope.Type;
            properties.MessageId = envelope.Key;
            properties.Timestamp = new AmqpTimestamp(new DateTimeOffset(envelope.OccurredOnUtc).ToUnixTimeSeconds());
            properties.Headers = MessagingTelemetry.CreatePropagationHeaders(envelope.Headers).ToDictionary(
                item => item.Key,
                item => (object)Encoding.UTF8.GetBytes(item.Value));

            var body = Encoding.UTF8.GetBytes(envelope.Payload);
            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: envelope.Destination,
                basicProperties: properties,
                body: body);
            channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));

            activity?.SetStatus(ActivityStatusCode.Ok);
            MessagingTelemetry.RecordPublished("rabbitmq", envelope.Destination, envelope.Type, Stopwatch.GetElapsedTime(started));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
            MessagingTelemetry.RecordPublishFailure(
                "rabbitmq",
                envelope.Destination,
                envelope.Type,
                Stopwatch.GetElapsedTime(started),
                exception.GetType().Name);
            throw;
        }

        return Task.CompletedTask;
    }
}
