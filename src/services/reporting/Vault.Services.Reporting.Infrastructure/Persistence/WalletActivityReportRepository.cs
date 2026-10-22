using MongoDB.Driver;
using Vault.Services.Reporting.Application.Abstractions.Persistence;
using Vault.Services.Reporting.Domain.Entities;
using Vault.Services.Reporting.Domain.Enums;
using Vault.Services.Reporting.Domain.ValueObjects;
using Vault.Services.Reporting.Infrastructure.Persistence.Documents;

namespace Vault.Services.Reporting.Infrastructure.Persistence;

public sealed class WalletActivityReportRepository(IMongoCollection<WalletActivityReportDocument> collection)
    : IWalletActivityReportRepository
{
    public async Task<bool> ExistsAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        var count = await collection.CountDocumentsAsync(x => x.Id == walletId, cancellationToken: cancellationToken);
        return count > 0;
    }

    public async Task<WalletActivityReport?> GetByIdAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        var document = await collection.Find(x => x.Id == walletId).FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : Map(document);
    }

    public async Task<IReadOnlyCollection<WalletActivityReport>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        var documents = await collection.Find(x => x.OwnerId == ownerId).ToListAsync(cancellationToken);
        return documents.Select(Map).ToArray();
    }

    public Task AddAsync(WalletActivityReport report, CancellationToken cancellationToken = default) =>
        collection.InsertOneAsync(ToDocument(report), cancellationToken: cancellationToken);

    public Task UpdateAsync(WalletActivityReport report, CancellationToken cancellationToken = default) =>
        collection.ReplaceOneAsync(x => x.Id == report.Id, ToDocument(report), cancellationToken: cancellationToken);

    private static WalletActivityReport Map(WalletActivityReportDocument document)
    {
        var currency = ReportingCurrency.Create(document.Currency).Value;
        var entries = document.Entries
            .Select(x => ActivityEntry.Create(Enum.Parse<ReportEntryType>(x.Type), x.Amount, x.Reference, x.Version, x.OccurredOnUtc).Value)
            .ToArray();

        return WalletActivityReport.Rehydrate(
            document.Id,
            document.OwnerId,
            currency,
            document.CurrentBalance,
            document.TotalDeposited,
            document.TotalWithdrawn,
            document.Version,
            document.OpenedOnUtc,
            document.LastUpdatedOnUtc,
            entries).Value;
    }

    private static WalletActivityReportDocument ToDocument(WalletActivityReport report) =>
        new()
        {
            Id = report.Id,
            OwnerId = report.OwnerId,
            Currency = report.Currency.Code,
            CurrentBalance = report.CurrentBalance,
            TotalDeposited = report.TotalDeposited,
            TotalWithdrawn = report.TotalWithdrawn,
            Version = report.Version,
            OpenedOnUtc = report.OpenedOnUtc,
            LastUpdatedOnUtc = report.LastUpdatedOnUtc,
            Entries = report.Entries
                .OrderBy(x => x.Version)
                .Select(x => new ActivityEntryDocument
                {
                    Type = x.Type.ToString(),
                    Amount = x.Amount,
                    Reference = x.Reference,
                    Version = x.Version,
                    OccurredOnUtc = x.OccurredOnUtc
                })
                .ToList()
        };
}
