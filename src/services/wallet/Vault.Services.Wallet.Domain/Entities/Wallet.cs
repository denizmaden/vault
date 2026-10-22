using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Wallet.Domain.Enums;
using Vault.Services.Wallet.Domain.Errors;
using Vault.Services.Wallet.Domain.ValueObjects;

namespace Vault.Services.Wallet.Domain.Entities;

public sealed class Wallet : AggregateRoot<Guid>
{
    private Wallet(
        Guid id,
        Guid ownerId,
        WalletCurrency currency,
        int version,
        DateTime openedOnUtc)
        : base(id)
    {
        OwnerId = ownerId;
        Currency = currency;
        Version = version;
        OpenedOnUtc = openedOnUtc;
        LastSyncedOnUtc = openedOnUtc;
        Status = WalletStatus.Active;
        Balance = 0m;
    }

    private Wallet()
        : base(Guid.Empty)
    {
        Currency = null!;
    }

    public Guid OwnerId { get; private set; }

    public WalletCurrency Currency { get; private set; }

    public decimal Balance { get; private set; }

    public WalletStatus Status { get; private set; }

    public int Version { get; private set; }

    public DateTime OpenedOnUtc { get; private set; }

    public DateTime LastSyncedOnUtc { get; private set; }

    public static Result<Wallet> Open(
        Guid walletId,
        Guid ownerId,
        WalletCurrency currency,
        int version,
        DateTime openedOnUtc)
    {
        if (walletId == Guid.Empty)
        {
            return Result<Wallet>.Failure(WalletErrors.InvalidWalletId);
        }

        if (ownerId == Guid.Empty)
        {
            return Result<Wallet>.Failure(WalletErrors.InvalidOwnerId);
        }

        if (version < 1)
        {
            return Result<Wallet>.Failure(WalletErrors.WalletVersionConflict);
        }

        return Result<Wallet>.Success(new Wallet(walletId, ownerId, currency, version, openedOnUtc));
    }

    public static Result<Wallet> Rehydrate(
        Guid walletId,
        Guid ownerId,
        WalletCurrency currency,
        decimal balance,
        WalletStatus status,
        int version,
        DateTime openedOnUtc,
        DateTime lastSyncedOnUtc)
    {
        if (walletId == Guid.Empty)
        {
            return Result<Wallet>.Failure(WalletErrors.InvalidWalletId);
        }

        if (ownerId == Guid.Empty)
        {
            return Result<Wallet>.Failure(WalletErrors.InvalidOwnerId);
        }

        if (version < 1)
        {
            return Result<Wallet>.Failure(WalletErrors.WalletVersionConflict);
        }

        if (balance < 0)
        {
            return Result<Wallet>.Failure(WalletErrors.InsufficientProjectedBalance);
        }

        var wallet = new Wallet(walletId, ownerId, currency, version, openedOnUtc)
        {
            Balance = balance,
            Status = status,
            LastSyncedOnUtc = lastSyncedOnUtc
        };

        return Result<Wallet>.Success(wallet);
    }

    public Result ApplyDeposit(WalletAmount amount, int version, DateTime occurredOnUtc)
    {
        if (version != Version + 1)
        {
            return Result.Failure(WalletErrors.WalletVersionConflict);
        }

        Balance += amount.Value;
        Version = version;
        LastSyncedOnUtc = occurredOnUtc;

        return Result.Success();
    }

    public Result ApplyWithdrawal(WalletAmount amount, int version, DateTime occurredOnUtc)
    {
        if (version != Version + 1)
        {
            return Result.Failure(WalletErrors.WalletVersionConflict);
        }

        if (Balance < amount.Value)
        {
            return Result.Failure(WalletErrors.InsufficientProjectedBalance);
        }

        Balance -= amount.Value;
        Version = version;
        LastSyncedOnUtc = occurredOnUtc;

        return Result.Success();
    }
}

