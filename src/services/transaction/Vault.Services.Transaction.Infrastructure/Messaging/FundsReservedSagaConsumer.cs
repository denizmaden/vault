using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Platform.Messaging.Abstractions;
using Vault.Services.Transaction.Application.Features.CompleteTransaction;
using Vault.Services.Transaction.Application.Features.FailTransaction;
using Vault.Services.Transaction.Infrastructure.Configuration;

namespace Vault.Services.Transaction.Infrastructure.Messaging;

public sealed class FundsReservedSagaConsumer(
    IMessageSubscriber subscriber,
    TransactionMessagingSettings settings,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<FundsReservedSagaConsumer> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        subscriber.SubscribeAsync(
            settings.ReservedQueue,
            "vault-transaction-funds-reserved",
            HandleMessageAsync,
            stoppingToken);

    private async Task HandleMessageAsync(MessageEnvelope envelope, CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Deserialize<TransactionFundsReservedMessage>(envelope.Payload)
            ?? throw new InvalidOperationException("Unable to deserialize funds reserved saga message.");

        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var commandDispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

        var completeResult = await commandDispatcher.DispatchAsync<CompleteTransactionCommand, Result>(
            new CompleteTransactionCommand(message.TransactionId),
            cancellationToken);

        if (completeResult.IsSuccess)
        {
            return;
        }

        await commandDispatcher.DispatchAsync<FailTransactionCommand, Result>(
            new FailTransactionCommand(message.TransactionId, completeResult.Error.Description),
            cancellationToken);

        logger.LogWarning(
            "Complete saga step failed for transaction {TransactionId} with error {ErrorCode}.",
            message.TransactionId,
            completeResult.Error.Code);
    }
}
