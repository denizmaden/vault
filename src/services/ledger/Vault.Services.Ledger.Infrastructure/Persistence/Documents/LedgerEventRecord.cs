namespace Vault.Services.Ledger.Infrastructure.Persistence.Documents;

public sealed class LedgerEventRecord
{
    public Guid Id { get; set; }

    public Guid LedgerAccountId { get; set; }

    public int Version { get; set; }

    public string EventType { get; set; } = null!;

    public string Data { get; set; } = null!;

    public DateTime OccurredOnUtc { get; set; }
}

