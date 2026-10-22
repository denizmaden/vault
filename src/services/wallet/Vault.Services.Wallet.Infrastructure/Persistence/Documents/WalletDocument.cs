using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Vault.Services.Wallet.Infrastructure.Persistence.Documents;

public sealed class WalletDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid OwnerId { get; set; }

    public string Currency { get; set; } = null!;

    public decimal Balance { get; set; }

    public string Status { get; set; } = null!;

    public int Version { get; set; }

    public DateTime OpenedOnUtc { get; set; }

    public DateTime LastSyncedOnUtc { get; set; }
}

