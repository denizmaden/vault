using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Transaction.Domain.Enums;
using Vault.Services.Transaction.Domain.Errors;
using Vault.Services.Transaction.Domain.Events;
using Vault.Services.Transaction.Domain.ValueObjects;

namespace Vault.Services.Transaction.Domain.Entities;

public sealed class TransactionRecord : AggregateRoot<Guid>
{
    private TransactionRecord(
        Guid id,
        Guid initiatingCustomerId,
        Guid sourceWalletId,
        Guid destinationWalletId,
        TransactionCurrency currency,
        TransactionAmount amount,
        string reference,
        DateTime createdOnUtc)
        : base(id)
    {
        InitiatingCustomerId = initiatingCustomerId;
        SourceWalletId = sourceWalletId;
        DestinationWalletId = destinationWalletId;
        Currency = currency;
        Amount = amount;
        Reference = reference;
        CreatedOnUtc = createdOnUtc;
        LastUpdatedOnUtc = createdOnUtc;
        Type = TransactionType.Transfer;
        Status = TransactionStatus.Requested;
    }

    private TransactionRecord()
        : base(Guid.Empty)
    {
        Currency = null!;
        Amount = null!;
        Reference = null!;
    }

    public Guid InitiatingCustomerId { get; private set; }

    public Guid SourceWalletId { get; private set; }

    public Guid DestinationWalletId { get; private set; }

    public TransactionCurrency Currency { get; private set; }

    public TransactionAmount Amount { get; private set; }

    public string Reference { get; private set; }

    public TransactionType Type { get; private set; }

    public TransactionStatus Status { get; private set; }

    public string? FailureReason { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    public DateTime LastUpdatedOnUtc { get; private set; }

    public DateTime? FundsReservedOnUtc { get; private set; }

    public DateTime? CompletedOnUtc { get; private set; }

    public DateTime? FailedOnUtc { get; private set; }

    public static Result<TransactionRecord> RequestTransfer(
        Guid transactionId,
        Guid initiatingCustomerId,
        Guid sourceWalletId,
        Guid destinationWalletId,
        TransactionCurrency currency,
        TransactionAmount amount,
        string? reference,
        DateTime createdOnUtc)
    {
        if (transactionId == Guid.Empty)
        {
            return Result<TransactionRecord>.Failure(TransactionErrors.InvalidTransactionId);
        }

        if (initiatingCustomerId == Guid.Empty)
        {
            return Result<TransactionRecord>.Failure(TransactionErrors.InvalidCustomerId);
        }

        if (sourceWalletId == Guid.Empty || destinationWalletId == Guid.Empty)
        {
            return Result<TransactionRecord>.Failure(TransactionErrors.InvalidWalletId);
        }

        if (sourceWalletId == destinationWalletId)
        {
            return Result<TransactionRecord>.Failure(TransactionErrors.DuplicateWallets);
        }

        var normalizedReference = NormalizeReference(reference);
        if (normalizedReference.IsFailure)
        {
            return Result<TransactionRecord>.Failure(normalizedReference.Error);
        }

        var transaction = new TransactionRecord(
            transactionId,
            initiatingCustomerId,
            sourceWalletId,
            destinationWalletId,
            currency,
            amount,
            normalizedReference.Value,
            createdOnUtc);

        transaction.RaiseDomainEvent(new TransferTransactionRequestedDomainEvent(
            transaction.Id,
            transaction.InitiatingCustomerId,
            transaction.SourceWalletId,
            transaction.DestinationWalletId,
            transaction.Currency.Code,
            transaction.Amount.Value,
            transaction.Reference));

        return Result<TransactionRecord>.Success(transaction);
    }

    public Result ReserveFunds(DateTime occurredOnUtc)
    {
        if (Status != TransactionStatus.Requested)
        {
            return Result.Failure(TransactionErrors.InvalidStatusTransition);
        }

        Status = TransactionStatus.FundsReserved;
        FundsReservedOnUtc = occurredOnUtc;
        LastUpdatedOnUtc = occurredOnUtc;
        RaiseDomainEvent(new TransactionFundsReservedDomainEvent(Id));

        return Result.Success();
    }

    public Result Complete(DateTime occurredOnUtc)
    {
        if (Status != TransactionStatus.FundsReserved)
        {
            return Result.Failure(TransactionErrors.InvalidStatusTransition);
        }

        Status = TransactionStatus.Completed;
        CompletedOnUtc = occurredOnUtc;
        LastUpdatedOnUtc = occurredOnUtc;
        RaiseDomainEvent(new TransactionCompletedDomainEvent(Id));

        return Result.Success();
    }

    public Result Fail(string? reason, DateTime occurredOnUtc)
    {
        if (Status is TransactionStatus.Completed or TransactionStatus.Failed)
        {
            return Result.Failure(TransactionErrors.InvalidStatusTransition);
        }

        var normalizedReason = NormalizeFailureReason(reason);
        if (normalizedReason.IsFailure)
        {
            return normalizedReason;
        }

        Status = TransactionStatus.Failed;
        FailureReason = normalizedReason.Value;
        FailedOnUtc = occurredOnUtc;
        LastUpdatedOnUtc = occurredOnUtc;
        RaiseDomainEvent(new TransactionFailedDomainEvent(Id, FailureReason));

        return Result.Success();
    }

    private static Result<string> NormalizeReference(string? reference)
    {
        var normalized = string.IsNullOrWhiteSpace(reference) ? "manual" : reference.Trim();
        if (normalized.Length > 150)
        {
            return Result<string>.Failure(TransactionErrors.InvalidReference);
        }

        return Result<string>.Success(normalized);
    }

    private static Result<string> NormalizeFailureReason(string? reason)
    {
        var normalized = string.IsNullOrWhiteSpace(reason) ? "unspecified" : reason.Trim();
        if (normalized.Length > 250)
        {
            return Result<string>.Failure(TransactionErrors.InvalidFailureReason);
        }

        return Result<string>.Success(normalized);
    }
}
