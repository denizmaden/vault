using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Transaction.Application.Abstractions.Persistence;
using Vault.Services.Transaction.Domain.Errors;

namespace Vault.Services.Transaction.Application.Features.CompleteTransaction;

public sealed class CompleteTransactionCommandHandler(
    ITransactionRepository repository) : ICommandHandler<CompleteTransactionCommand, Result>
{
    public async Task<Result> Handle(CompleteTransactionCommand command, CancellationToken cancellationToken = default)
    {
        var transaction = await repository.GetByIdAsync(command.TransactionId, cancellationToken);
        if (transaction is null)
        {
            return Result.Failure(TransactionErrors.TransactionNotFound);
        }

        var result = transaction.Complete(DateTime.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
