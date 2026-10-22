using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Ledger.Domain.Enums;
using Vault.Services.Ledger.Domain.Errors;
using Vault.Services.Ledger.Domain.Events;
using Vault.Services.Ledger.Domain.ValueObjects;

namespace Vault.Services.Ledger.Domain.Entities;

public sealed class LedgerAccount : AggregateRoot<Guid>
{
    private readonly List<LedgerAccountEvent> _uncommittedEvents = [];

    private LedgerAccount(Guid id)
        : base(id)
    {
        Currency = null!;
    }

    private LedgerAccount()
        : base(Guid.Empty)
    {
        Currency = null!;
    }

    public Guid OwnerId { get; private set; }

    public Currency Currency { get; private set; }

    public decimal Balance { get; private set; }

    public LedgerAccountStatus Status { get; private set; }

    public DateTime OpenedOnUtc { get; private set; }

    public int Version { get; private set; }

    public IReadOnlyCollection<LedgerAccountEvent> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    public void ClearUncommittedEvents()
    {
        _uncommittedEvents.Clear();
        ClearDomainEvents();
    }

    public static Result<LedgerAccount> Open(
        Guid ledgerAccountId,
        Guid ownerId,
        Currency currency,
        DateTime openedOnUtc)
    {
        if (ledgerAccountId == Guid.Empty)
        {
            return Result<LedgerAccount>.Failure(LedgerErrors.InvalidLedgerAccountId);
        }

        if (ownerId == Guid.Empty)
        {
            return Result<LedgerAccount>.Failure(LedgerErrors.InvalidOwnerId);
        }

        var account = new LedgerAccount(ledgerAccountId);
        account.Record(new LedgerAccountOpenedEvent(ledgerAccountId, ownerId, currency.Code, openedOnUtc));

        return Result<LedgerAccount>.Success(account);
    }

    public static Result<LedgerAccount> Rehydrate(
        Guid ledgerAccountId,
        IReadOnlyCollection<LedgerAccountEvent> events)
    {
        if (ledgerAccountId == Guid.Empty)
        {
            return Result<LedgerAccount>.Failure(LedgerErrors.InvalidLedgerAccountId);
        }

        if (events.Count == 0)
        {
            return Result<LedgerAccount>.Failure(LedgerErrors.InvalidEventStream);
        }

        var account = new LedgerAccount(ledgerAccountId);

        foreach (var ledgerEvent in events)
        {
            account.Apply(ledgerEvent);
        }

        account.ClearUncommittedEvents();

        return Result<LedgerAccount>.Success(account);
    }

    public Result Deposit(MoneyAmount amount, string reference, DateTime occurredOnUtc)
    {
        if (Status != LedgerAccountStatus.Active)
        {
            return Result.Failure(LedgerErrors.InvalidEventStream);
        }

        Record(new FundsDepositedEvent(Id, amount.Value, NormalizeReference(reference), occurredOnUtc));
        return Result.Success();
    }

    public Result Withdraw(MoneyAmount amount, string reference, DateTime occurredOnUtc)
    {
        if (Status != LedgerAccountStatus.Active)
        {
            return Result.Failure(LedgerErrors.InvalidEventStream);
        }

        if (Balance < amount.Value)
        {
            return Result.Failure(LedgerErrors.InsufficientFunds);
        }

        Record(new FundsWithdrawnEvent(Id, amount.Value, NormalizeReference(reference), occurredOnUtc));
        return Result.Success();
    }

    private void Record(LedgerAccountEvent ledgerEvent)
    {
        Apply(ledgerEvent);
        _uncommittedEvents.Add(ledgerEvent);
        RaiseDomainEvent(ledgerEvent);
    }

    private void Apply(LedgerAccountEvent ledgerEvent)
    {
        switch (ledgerEvent)
        {
            case LedgerAccountOpenedEvent opened:
                var currencyResult = Currency.Create(opened.Currency);

                if (currencyResult.IsFailure)
                {
                    throw new InvalidOperationException(currencyResult.Error.Description);
                }

                OwnerId = opened.OwnerId;
                Currency = currencyResult.Value;
                Balance = 0m;
                Status = LedgerAccountStatus.Active;
                OpenedOnUtc = opened.OccurredOnUtc;
                Version++;
                break;

            case FundsDepositedEvent deposited:
                Balance += deposited.Amount;
                Version++;
                break;

            case FundsWithdrawnEvent withdrawn:
                Balance -= withdrawn.Amount;
                Version++;
                break;

            default:
                throw new InvalidOperationException(LedgerErrors.UnsupportedEventType.Description);
        }
    }

    private static string NormalizeReference(string reference)
    {
        return string.IsNullOrWhiteSpace(reference)
            ? "manual"
            : reference.Trim();
    }
}

