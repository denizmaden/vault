using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Platform.Messaging.Abstractions;
using Vault.Services.Wallet.Application.Features.ProjectFundsDeposited;
using Vault.Services.Wallet.Application.Features.ProjectFundsWithdrawn;
using Vault.Services.Wallet.Application.Features.ProjectLedgerAccountOpened;
using Vault.Services.Wallet.Infrastructure.Configuration;

namespace Vault.Services.Wallet.Infrastructure.Messaging;

public sealed class LedgerEventsConsumer(
    IMessageSubscriber subscriber,
    WalletMessagingSettings settings,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<LedgerEventsConsumer> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return subscriber.SubscribeAsync(
            settings.LedgerEventsDestination,
            settings.ConsumerGroupId,
            HandleMessageAsync,
            stoppingToken);
    }

    private async Task HandleMessageAsync(MessageEnvelope message, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(message.Type) || message.Type == "unknown")
        {
            logger.LogWarning("Ledger event message type is missing.");
            return;
        }

        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

        switch (message.Type)
        {
            case "LedgerAccountOpenedEvent":
            {
                var @event = JsonSerializer.Deserialize<LedgerAccountOpenedIntegrationEvent>(message.Payload, JsonSerializerOptions)
                    ?? throw new InvalidOperationException("Unable to deserialize ledger account opened event.");
                var version = ReadInt32Header(message.Headers, "event-version") ?? 1;

                var result = await dispatcher.DispatchAsync<ProjectLedgerAccountOpenedCommand, Result>(
                    new ProjectLedgerAccountOpenedCommand(
                        @event.LedgerAccountId,
                        @event.OwnerId,
                        @event.Currency,
                        version,
                        @event.OccurredOnUtc),
                    cancellationToken);
                LogFailureIfNeeded(result, message.Type, @event.LedgerAccountId);
                break;
            }

            case "FundsDepositedEvent":
            {
                var @event = JsonSerializer.Deserialize<FundsDepositedIntegrationEvent>(message.Payload, JsonSerializerOptions)
                    ?? throw new InvalidOperationException("Unable to deserialize funds deposited event.");
                var version = ReadInt32Header(message.Headers, "event-version")
                    ?? throw new InvalidOperationException("Event version header is missing for funds deposited event.");

                var result = await dispatcher.DispatchAsync<ProjectFundsDepositedCommand, Result>(
                    new ProjectFundsDepositedCommand(
                        @event.LedgerAccountId,
                        @event.Amount,
                        version,
                        @event.OccurredOnUtc),
                    cancellationToken);
                LogFailureIfNeeded(result, message.Type, @event.LedgerAccountId);
                break;
            }

            case "FundsWithdrawnEvent":
            {
                var @event = JsonSerializer.Deserialize<FundsWithdrawnIntegrationEvent>(message.Payload, JsonSerializerOptions)
                    ?? throw new InvalidOperationException("Unable to deserialize funds withdrawn event.");
                var version = ReadInt32Header(message.Headers, "event-version")
                    ?? throw new InvalidOperationException("Event version header is missing for funds withdrawn event.");

                var result = await dispatcher.DispatchAsync<ProjectFundsWithdrawnCommand, Result>(
                    new ProjectFundsWithdrawnCommand(
                        @event.LedgerAccountId,
                        @event.Amount,
                        version,
                        @event.OccurredOnUtc),
                    cancellationToken);
                LogFailureIfNeeded(result, message.Type, @event.LedgerAccountId);
                break;
            }

            default:
                logger.LogInformation("Skipping unsupported ledger event type {MessageType}.", message.Type);
                break;
        }
    }

    private void LogFailureIfNeeded(Result result, string messageType, Guid walletId)
    {
        if (result.IsFailure)
        {
            logger.LogWarning(
                "Wallet projection command for message type {MessageType} completed with domain failure {ErrorCode} for wallet {WalletId}.",
                messageType,
                result.Error.Code,
                walletId);
        }
    }

    private static int? ReadInt32Header(IReadOnlyDictionary<string, string>? headers, string name)
    {
        return headers is not null &&
               headers.TryGetValue(name, out var value) &&
               int.TryParse(value, out var parsed)
            ? parsed
            : null;
    }
}
