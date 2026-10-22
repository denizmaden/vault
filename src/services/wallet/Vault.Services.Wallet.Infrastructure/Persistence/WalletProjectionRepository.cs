using MongoDB.Driver;
using Vault.Services.Wallet.Application.Abstractions.Persistence;
using Vault.Services.Wallet.Application.Models;
using Vault.Services.Wallet.Domain.Enums;
using Vault.Services.Wallet.Domain.ValueObjects;
using Vault.Services.Wallet.Infrastructure.Persistence.Documents;
using WalletProjection = Vault.Services.Wallet.Domain.Entities.Wallet;

namespace Vault.Services.Wallet.Infrastructure.Persistence;

public sealed class WalletProjectionRepository(IMongoCollection<WalletDocument> walletsCollection) : IWalletProjectionRepository
{
    public async Task<bool> ExistsAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        var count = await walletsCollection.CountDocumentsAsync(
            Builders<WalletDocument>.Filter.Eq(x => x.Id, walletId),
            cancellationToken: cancellationToken);

        return count > 0;
    }

    public async Task<WalletProjection?> GetAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        var document = await walletsCollection
            .Find(x => x.Id == walletId)
            .SingleOrDefaultAsync(cancellationToken);

        return document is null ? null : ToDomain(document);
    }

    public async Task<IReadOnlyCollection<WalletDetails>> GetByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        var documents = await walletsCollection
            .Find(x => x.OwnerId == ownerId)
            .SortBy(x => x.Currency)
            .ToListAsync(cancellationToken);

        return documents
            .Select(x => new WalletDetails(
                x.Id,
                x.OwnerId,
                x.Currency,
                x.Balance,
                x.Status,
                x.Version,
                x.OpenedOnUtc,
                x.LastSyncedOnUtc))
            .ToArray();
    }

    public Task UpsertAsync(WalletProjection wallet, CancellationToken cancellationToken = default)
    {
        var document = new WalletDocument
        {
            Id = wallet.Id,
            OwnerId = wallet.OwnerId,
            Currency = wallet.Currency.Code,
            Balance = wallet.Balance,
            Status = wallet.Status.ToString(),
            Version = wallet.Version,
            OpenedOnUtc = wallet.OpenedOnUtc,
            LastSyncedOnUtc = wallet.LastSyncedOnUtc
        };

        return walletsCollection.ReplaceOneAsync(
            x => x.Id == wallet.Id,
            document,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);
    }

    private static WalletProjection ToDomain(WalletDocument document)
    {
        var currency = WalletCurrency.Create(document.Currency).Value;
        var status = Enum.Parse<WalletStatus>(document.Status);
        return WalletProjection.Rehydrate(
            document.Id,
            document.OwnerId,
            currency,
            document.Balance,
            status,
            document.Version,
            document.OpenedOnUtc,
            document.LastSyncedOnUtc).Value;
    }
}

