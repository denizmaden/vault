using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vault.Platform.Messaging.Abstractions;
using Vault.Platform.Messaging.Abstractions.Telemetry;
using Vault.Services.Ledger.Infrastructure.Configuration;
using Vault.Services.Ledger.Infrastructure.Persistence;

namespace Vault.Services.Ledger.Infrastructure.Messaging;

public sealed class LedgerOutboxDispatcher(
    IServiceScopeFactory serviceScopeFactory,
    LedgerMessagingSettings settings,
    ILogger<LedgerOutboxDispatcher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchBatchAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Ledger outbox dispatch failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task DispatchBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LedgerDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

        var messages = await dbContext.OutboxMessages
            .Where(message => message.ProcessedOnUtc == null)
            .OrderBy(message => message.OccurredOnUtc)
            .Take(50)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            var key = TryExtractLedgerAccountId(message.Payload) ?? message.Id.ToString("D");
            var version = await ResolveEventVersionAsync(dbContext, message, cancellationToken);

            try
            {
                await publisher.PublishAsync(
                    new MessageEnvelope(
                        settings.EventsDestination,
                        key,
                        message.Type,
                        message.Payload,
                        message.OccurredOnUtc,
                        new Dictionary<string, string>
                        {
                            ["event-version"] = version.ToString()
                        }),
                    cancellationToken);

                MessagingTelemetry.RecordOutboxDispatched("Vault.Ledger", settings.EventsDestination, message.Type, 1);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                MessagingTelemetry.RecordOutboxFailure(
                    "Vault.Ledger",
                    settings.EventsDestination,
                    message.Type,
                    exception.GetType().Name);
                throw;
            }

            message.ProcessedOnUtc = DateTime.UtcNow;
        }

        if (messages.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static string? TryExtractLedgerAccountId(string payload)
    {
        using var document = JsonDocument.Parse(payload);
        return document.RootElement.TryGetProperty("ledgerAccountId", out var property)
            ? property.GetGuid().ToString("D")
            : document.RootElement.TryGetProperty("LedgerAccountId", out property)
            ? property.GetGuid().ToString("D")
            : null;
    }

    private static async Task<int> ResolveEventVersionAsync(
        LedgerDbContext dbContext,
        Persistence.Documents.OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var version = await dbContext.LedgerEvents
            .Where(ledgerEvent =>
                ledgerEvent.EventType == message.Type &&
                ledgerEvent.Data == message.Payload &&
                ledgerEvent.OccurredOnUtc == message.OccurredOnUtc)
            .Select(ledgerEvent => (int?)ledgerEvent.Version)
            .SingleOrDefaultAsync(cancellationToken);

        return version ?? 1;
    }
}

