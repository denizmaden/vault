using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Reporting.Domain.Enums;
using Vault.Services.Reporting.Domain.Errors;
using Vault.Services.Reporting.Domain.ValueObjects;

namespace Vault.Services.Reporting.Domain.Entities;

public sealed class WalletActivityReport : AggregateRoot<Guid>
{
    private readonly List<ActivityEntry> _entries = [];

    private WalletActivityReport(
        Guid walletId,
        Guid ownerId,
        ReportingCurrency currency,
        int version,
        DateTime openedOnUtc)
        : base(walletId)
    {
        OwnerId = ownerId;
        Currency = currency;
        Version = version;
        OpenedOnUtc = openedOnUtc;
        LastUpdatedOnUtc = openedOnUtc;
    }

    private WalletActivityReport()
        : base(Guid.Empty)
    {
        Currency = null!;
    }

    public Guid OwnerId { get; private set; }

    public ReportingCurrency Currency { get; private set; }

    public decimal CurrentBalance { get; private set; }

    public decimal TotalDeposited { get; private set; }

    public decimal TotalWithdrawn { get; private set; }

    public int Version { get; private set; }

    public DateTime OpenedOnUtc { get; private set; }

    public DateTime LastUpdatedOnUtc { get; private set; }

    public IReadOnlyCollection<ActivityEntry> Entries => _entries.AsReadOnly();

    public static Result<WalletActivityReport> Open(
        Guid walletId,
        Guid ownerId,
        ReportingCurrency currency,
        int version,
        DateTime openedOnUtc)
    {
        if (walletId == Guid.Empty)
        {
            return Result<WalletActivityReport>.Failure(ReportingErrors.InvalidWalletId);
        }

        if (ownerId == Guid.Empty)
        {
            return Result<WalletActivityReport>.Failure(ReportingErrors.InvalidOwnerId);
        }

        if (version < 1)
        {
            return Result<WalletActivityReport>.Failure(ReportingErrors.InvalidVersion);
        }

        var report = new WalletActivityReport(walletId, ownerId, currency, version, openedOnUtc);
        var openedEntry = ActivityEntry.Create(ReportEntryType.Opened, 0m, "wallet-opened", version, openedOnUtc);
        if (openedEntry.IsFailure)
        {
            return Result<WalletActivityReport>.Failure(openedEntry.Error);
        }

        report._entries.Add(openedEntry.Value);

        return Result<WalletActivityReport>.Success(report);
    }

    public static Result<WalletActivityReport> Rehydrate(
        Guid walletId,
        Guid ownerId,
        ReportingCurrency currency,
        decimal currentBalance,
        decimal totalDeposited,
        decimal totalWithdrawn,
        int version,
        DateTime openedOnUtc,
        DateTime lastUpdatedOnUtc,
        IReadOnlyCollection<ActivityEntry> entries)
    {
        if (walletId == Guid.Empty)
        {
            return Result<WalletActivityReport>.Failure(ReportingErrors.InvalidWalletId);
        }

        if (ownerId == Guid.Empty)
        {
            return Result<WalletActivityReport>.Failure(ReportingErrors.InvalidOwnerId);
        }

        if (version < 1)
        {
            return Result<WalletActivityReport>.Failure(ReportingErrors.InvalidVersion);
        }

        if (currentBalance < 0)
        {
            return Result<WalletActivityReport>.Failure(ReportingErrors.NegativeProjectedBalance);
        }

        var report = new WalletActivityReport(walletId, ownerId, currency, version, openedOnUtc)
        {
            CurrentBalance = currentBalance,
            TotalDeposited = totalDeposited,
            TotalWithdrawn = totalWithdrawn,
            LastUpdatedOnUtc = lastUpdatedOnUtc
        };

        report._entries.AddRange(entries.OrderBy(x => x.Version));

        return Result<WalletActivityReport>.Success(report);
    }

    public Result ApplyDeposit(ReportingAmount amount, string? reference, int version, DateTime occurredOnUtc)
    {
        if (version != Version + 1)
        {
            return Result.Failure(ReportingErrors.VersionConflict);
        }

        var entry = ActivityEntry.Create(ReportEntryType.Deposited, amount.Value, reference, version, occurredOnUtc);
        if (entry.IsFailure)
        {
            return Result.Failure(entry.Error);
        }

        CurrentBalance += amount.Value;
        TotalDeposited += amount.Value;
        Version = version;
        LastUpdatedOnUtc = occurredOnUtc;
        _entries.Add(entry.Value);

        return Result.Success();
    }

    public Result ApplyWithdrawal(ReportingAmount amount, string? reference, int version, DateTime occurredOnUtc)
    {
        if (version != Version + 1)
        {
            return Result.Failure(ReportingErrors.VersionConflict);
        }

        if (CurrentBalance - amount.Value < 0)
        {
            return Result.Failure(ReportingErrors.NegativeProjectedBalance);
        }

        var entry = ActivityEntry.Create(ReportEntryType.Withdrawn, amount.Value, reference, version, occurredOnUtc);
        if (entry.IsFailure)
        {
            return Result.Failure(entry.Error);
        }

        CurrentBalance -= amount.Value;
        TotalWithdrawn += amount.Value;
        Version = version;
        LastUpdatedOnUtc = occurredOnUtc;
        _entries.Add(entry.Value);

        return Result.Success();
    }
}
