using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vault.Platform.Messaging.Abstractions;
using Vault.Platform.Messaging.Abstractions.Telemetry;
using Vault.Services.Transaction.Domain.Events;
using Vault.Services.Transaction.Infrastructure.Configuration;
using Vault.Services.Transaction.Infrastructure.Persistence;

namespace Vault.Services.Transaction.Infrastructure.Messaging;

public sealed class TransactionOutboxDispatcher(
    IServiceScopeFactory serviceScopeFactory,
    TransactionMessagingSettings settings,
    IMessagePublisher messagePublisher,
    ILogger<TransactionOutboxDispatcher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchPendingMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Transaction outbox dispatcher failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task DispatchPendingMessagesAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        var messages = await dbContext.OutboxMessages
            .Where(message => message.ProcessedOnUtc == null)
            .OrderBy(message => message.OccurredOnUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            var envelope = CreateEnvelope(message, settings);
            if (envelope is null)
            {
                message.ProcessedOnUtc = DateTime.UtcNow;
                continue;
            }

            try
            {
                await messagePublisher.PublishAsync(envelope, cancellationToken);
                MessagingTelemetry.RecordOutboxDispatched("Vault.Transaction", envelope.Destination, envelope.Type, 1);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                MessagingTelemetry.RecordOutboxFailure(
                    "Vault.Transaction",
                    envelope.Destination,
                    envelope.Type,
                    exception.GetType().Name);
                throw;
            }

            message.ProcessedOnUtc = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static MessageEnvelope? CreateEnvelope(Persistence.Documents.OutboxMessage message, TransactionMessagingSettings settings)
    {
        return message.Type switch
        {
            nameof(TransferTransactionRequestedDomainEvent) => CreateRequestedEnvelope(message, settings),
            nameof(TransactionFundsReservedDomainEvent) => CreateReservedEnvelope(message, settings),
            nameof(TransactionCompletedDomainEvent) => CreateCompletedEnvelope(message, settings),
            nameof(TransactionFailedDomainEvent) => CreateFailedEnvelope(message, settings),
            _ => null
        };
    }

    private static MessageEnvelope CreateRequestedEnvelope(Persistence.Documents.OutboxMessage message, TransactionMessagingSettings settings)
    {
        var domainEvent = JsonSerializer.Deserialize<TransferTransactionRequestedDomainEvent>(message.Payload)
            ?? throw new InvalidOperationException("Unable to deserialize transfer requested domain event.");

        var payload = JsonSerializer.Serialize(new TransferTransactionRequestedMessage(
            domainEvent.TransactionId,
            domainEvent.InitiatingCustomerId,
            domainEvent.SourceWalletId,
            domainEvent.DestinationWalletId,
            domainEvent.Currency,
            domainEvent.Amount,
            domainEvent.Reference,
            domainEvent.OccurredOnUtc));

        return CreateEnvelope(settings.RequestedQueue, message, payload);
    }

    private static MessageEnvelope CreateReservedEnvelope(Persistence.Documents.OutboxMessage message, TransactionMessagingSettings settings)
    {
        var domainEvent = JsonSerializer.Deserialize<TransactionFundsReservedDomainEvent>(message.Payload)
            ?? throw new InvalidOperationException("Unable to deserialize funds reserved domain event.");

        var payload = JsonSerializer.Serialize(new TransactionFundsReservedMessage(
            domainEvent.TransactionId,
            domainEvent.OccurredOnUtc));

        return CreateEnvelope(settings.ReservedQueue, message, payload);
    }

    private static MessageEnvelope CreateCompletedEnvelope(Persistence.Documents.OutboxMessage message, TransactionMessagingSettings settings)
    {
        var domainEvent = JsonSerializer.Deserialize<TransactionCompletedDomainEvent>(message.Payload)
            ?? throw new InvalidOperationException("Unable to deserialize completed domain event.");

        var payload = JsonSerializer.Serialize(new TransactionCompletedMessage(
            domainEvent.TransactionId,
            domainEvent.OccurredOnUtc));

        return CreateEnvelope(settings.CompletedQueue, message, payload);
    }

    private static MessageEnvelope CreateFailedEnvelope(Persistence.Documents.OutboxMessage message, TransactionMessagingSettings settings)
    {
        var domainEvent = JsonSerializer.Deserialize<TransactionFailedDomainEvent>(message.Payload)
            ?? throw new InvalidOperationException("Unable to deserialize failed domain event.");

        var payload = JsonSerializer.Serialize(new TransactionFailedMessage(
            domainEvent.TransactionId,
            domainEvent.FailureReason,
            domainEvent.OccurredOnUtc));

        return CreateEnvelope(settings.FailedQueue, message, payload);
    }

    private static MessageEnvelope CreateEnvelope(string queueName, Persistence.Documents.OutboxMessage message, string payload) =>
        new(
            queueName,
            message.Id.ToString("N"),
            message.Type,
            payload,
            message.OccurredOnUtc,
            new Dictionary<string, string>
            {
                ["message-type"] = message.Type
            });
}
