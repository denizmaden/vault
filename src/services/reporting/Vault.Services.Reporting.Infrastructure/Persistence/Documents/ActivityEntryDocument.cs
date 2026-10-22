using MongoDB.Bson.Serialization.Attributes;

namespace Vault.Services.Reporting.Infrastructure.Persistence.Documents;

public sealed class ActivityEntryDocument
{
    public string Type { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Reference { get; set; } = null!;

    public int Version { get; set; }

    public DateTime OccurredOnUtc { get; set; }
}
