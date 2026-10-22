using System.Text.Json;
using Vault.Services.Ledger.Domain.Events;

namespace Vault.Services.Ledger.Infrastructure.Persistence;

internal static class LedgerEventSerializer
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public static string GetEventType(LedgerAccountEvent ledgerEvent) => ledgerEvent.GetType().Name;

    public static string Serialize(LedgerAccountEvent ledgerEvent)
        => JsonSerializer.Serialize(ledgerEvent, ledgerEvent.GetType(), JsonSerializerOptions);

    public static LedgerAccountEvent Deserialize(string eventType, string payload)
    {
        LedgerAccountEvent? eventInstance = eventType switch
        {
            nameof(LedgerAccountOpenedEvent) => JsonSerializer.Deserialize<LedgerAccountOpenedEvent>(payload, JsonSerializerOptions),
            nameof(FundsDepositedEvent) => JsonSerializer.Deserialize<FundsDepositedEvent>(payload, JsonSerializerOptions),
            nameof(FundsWithdrawnEvent) => JsonSerializer.Deserialize<FundsWithdrawnEvent>(payload, JsonSerializerOptions),
            _ => null
        };

        return eventInstance
            ?? throw new InvalidOperationException($"Unsupported ledger event type '{eventType}'.");
    }
}

