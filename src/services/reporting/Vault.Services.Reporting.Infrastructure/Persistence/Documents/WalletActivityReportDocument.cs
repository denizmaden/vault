using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Vault.Services.Reporting.Infrastructure.Persistence.Documents;

public sealed class WalletActivityReportDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid OwnerId { get; set; }

    public string Currency { get; set; } = null!;

    public decimal CurrentBalance { get; set; }

    public decimal TotalDeposited { get; set; }

    public decimal TotalWithdrawn { get; set; }

    public int Version { get; set; }

    public DateTime OpenedOnUtc { get; set; }

    public DateTime LastUpdatedOnUtc { get; set; }

    public List<ActivityEntryDocument> Entries { get; set; } = [];
}
