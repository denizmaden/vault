namespace Vault.Services.Ledger.Infrastructure.Persistence.Documents;

public sealed class LedgerAccountProjection
{
    public Guid LedgerAccountId { get; set; }

    public Guid OwnerId { get; set; }

    public string Currency { get; set; } = null!;

    public decimal Balance { get; set; }

    public string Status { get; set; } = null!;

    public int Version { get; set; }

    public DateTime OpenedOnUtc { get; set; }

    public DateTime LastUpdatedOnUtc { get; set; }
}

