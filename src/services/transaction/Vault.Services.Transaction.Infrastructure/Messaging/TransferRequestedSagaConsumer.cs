using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Platform.Messaging.Abstractions;
using Vault.Services.Transaction.Application.Features.FailTransaction;
using Vault.Services.Transaction.Application.Features.ReserveTransactionFunds;
using Vault.Services.Transaction.Infrastructure.Configuration;

namespace Vault.Services.Transaction.Infrastructure.Messaging;

public sealed class TransferRequestedSagaConsumer(
    IMessageSubscriber subscriber,
    TransactionMessagingSettings settings,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<TransferRequestedSagaConsumer> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        subscriber.SubscribeAsync(
            settings.RequestedQueue,
            "vault-transaction-transfer-requested",
            HandleMessageAsync,
            stoppingToken);

    private async Task HandleMessageAsync(MessageEnvelope envelope, CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Deserialize<TransferTransactionRequestedMessage>(envelope.Payload)
            ?? throw new InvalidOperationException("Unable to deserialize transfer requested saga message.");

        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var commandDispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

        var reserveResult = await commandDispatcher.DispatchAsync<ReserveTransactionFundsCommand, Result>(
            new ReserveTransactionFundsCommand(message.TransactionId),
            cancellationToken);

        if (reserveResult.IsSuccess)
        {
            return;
        }

        await commandDispatcher.DispatchAsync<FailTransactionCommand, Result>(
            new FailTransactionCommand(message.TransactionId, reserveResult.Error.Description),
            cancellationToken);

        logger.LogWarning(
            "Reserve saga step failed for transaction {TransactionId} with error {ErrorCode}.",
            message.TransactionId,
            reserveResult.Error.Code);
    }
}
