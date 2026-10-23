using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vault.Platform.Messaging.Abstractions;
using Vault.Services.MarketData.Application.Abstractions;
using Vault.Services.MarketData.Infrastructure.Configuration;

namespace Vault.Services.MarketData.Infrastructure.Messaging;

public sealed class MarketPriceProjectionConsumer(
    IMessageSubscriber subscriber,
    IMessagePublisher publisher,
    IMarketPriceSnapshotStore snapshotStore,
    IMarketPriceRealtimeBackplane backplane,
    MarketDataMessagingSettings settings,
    ILogger<MarketPriceProjectionConsumer> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        subscriber.SubscribeAsync(
            settings.PricesDestination,
            settings.ProjectionConsumerGroupId,
            HandleMessageAsync,
            stoppingToken);

    private async Task HandleMessageAsync(MessageEnvelope envelope, CancellationToken cancellationToken)
    {
        if (!string.Equals(
                envelope.Type,
                nameof(MarketPriceUpdatedIntegrationEvent),
                StringComparison.Ordinal))
        {
            logger.LogInformation(
                "MarketData projection skipped unsupported message type {MessageType}.",
                envelope.Type);
            return;
        }

        Exception? lastException = null;
        for (var attempt = 1; attempt <= settings.MaxProcessingAttempts; attempt++)
        {
            try
            {
                await ProcessAsync(envelope, cancellationToken);
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                lastException = exception;
                logger.LogWarning(
                    exception,
                    "MarketData projection attempt {Attempt}/{MaxAttempts} failed for key {MessageKey}.",
                    attempt,
                    settings.MaxProcessingAttempts,
                    envelope.Key);

                if (attempt < settings.MaxProcessingAttempts)
                {
                    var multiplier = Math.Pow(2, attempt - 1);
                    var delay = TimeSpan.FromMilliseconds(
                        Math.Min(settings.RetryInitialDelayMilliseconds * multiplier, 10_000));
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }

        await PublishToDeadLetterUntilSuccessfulAsync(
            envelope,
            lastException ?? new InvalidOperationException("MarketData projection failed."),
            cancellationToken);
    }

    private async Task ProcessAsync(MessageEnvelope envelope, CancellationToken cancellationToken)
    {
        var @event = JsonSerializer.Deserialize<MarketPriceUpdatedIntegrationEvent>(
            envelope.Payload,
            JsonSerializerOptions)
            ?? throw new InvalidOperationException("Unable to deserialize market price event.");

        var price = @event.ToDomain();
        var accepted = await snapshotStore.UpsertAsync(price, cancellationToken);
        if (accepted)
        {
            await backplane.PublishAsync(price, cancellationToken);
        }
        else
        {
            logger.LogDebug(
                "MarketData projection ignored an older or duplicate price for {Symbol} received at {ReceivedOnUtc}.",
                price.Instrument.Symbol,
                price.ReceivedOnUtc);
        }
    }

    private async Task PublishToDeadLetterUntilSuccessfulAsync(
        MessageEnvelope originalEnvelope,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var deadLetter = new MarketPriceProjectionFailedEvent(
            originalEnvelope.Destination,
            originalEnvelope.Key,
            originalEnvelope.Type,
            originalEnvelope.Payload,
            originalEnvelope.OccurredOnUtc,
            originalEnvelope.Headers,
            settings.MaxProcessingAttempts,
            exception.GetType().Name,
            exception.Message,
            DateTime.UtcNow);
        var envelope = new MessageEnvelope(
            settings.DeadLetterDestination,
            originalEnvelope.Key,
            nameof(MarketPriceProjectionFailedEvent),
            JsonSerializer.Serialize(deadLetter, JsonSerializerOptions),
            DateTime.UtcNow,
            new Dictionary<string, string>
            {
                ["original-destination"] = originalEnvelope.Destination,
                ["original-message-type"] = originalEnvelope.Type
            });

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await publisher.PublishAsync(envelope, cancellationToken);
                logger.LogError(
                    exception,
                    "MarketData message {MessageKey} moved to dead-letter destination {DeadLetterDestination} after {AttemptCount} attempts.",
                    originalEnvelope.Key,
                    settings.DeadLetterDestination,
                    settings.MaxProcessingAttempts);
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception publishException)
            {
                logger.LogError(
                    publishException,
                    "Publishing MarketData message {MessageKey} to dead-letter destination failed. Retrying.",
                    originalEnvelope.Key);
                await Task.Delay(
                    TimeSpan.FromSeconds(settings.DeadLetterPublishRetrySeconds),
                    cancellationToken);
            }
        }
    }
}
