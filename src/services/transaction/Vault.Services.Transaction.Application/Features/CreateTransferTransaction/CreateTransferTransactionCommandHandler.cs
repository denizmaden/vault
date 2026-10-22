using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Transaction.Application.Abstractions.Persistence;
using Vault.Services.Transaction.Application.Models;
using Vault.Services.Transaction.Domain.Entities;
using Vault.Services.Transaction.Domain.Errors;
using Vault.Services.Transaction.Domain.ValueObjects;

namespace Vault.Services.Transaction.Application.Features.CreateTransferTransaction;

public sealed class CreateTransferTransactionCommandHandler(
    ITransactionRepository repository) : ICommandHandler<CreateTransferTransactionCommand, Result<TransactionCreated>>
{
    public async Task<Result<TransactionCreated>> Handle(
        CreateTransferTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        if (await repository.ExistsAsync(command.TransactionId, cancellationToken))
        {
            return Result<TransactionCreated>.Failure(TransactionErrors.DuplicateTransaction);
        }

        var currencyResult = TransactionCurrency.Create(command.Currency);
        if (currencyResult.IsFailure)
        {
            return Result<TransactionCreated>.Failure(currencyResult.Error);
        }

        var amountResult = TransactionAmount.Create(command.Amount);
        if (amountResult.IsFailure)
        {
            return Result<TransactionCreated>.Failure(amountResult.Error);
        }

        var transactionResult = TransactionRecord.RequestTransfer(
            command.TransactionId,
            command.InitiatingCustomerId,
            command.SourceWalletId,
            command.DestinationWalletId,
            currencyResult.Value,
            amountResult.Value,
            command.Reference,
            DateTime.UtcNow);

        if (transactionResult.IsFailure)
        {
            return Result<TransactionCreated>.Failure(transactionResult.Error);
        }

        await repository.AddAsync(transactionResult.Value, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Result<TransactionCreated>.Success(new TransactionCreated(transactionResult.Value.Id, transactionResult.Value.Status.ToString()));
    }
}
