using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Transaction.Application.Abstractions.Persistence;
using Vault.Services.Transaction.Domain.Errors;

namespace Vault.Services.Transaction.Application.Features.FailTransaction;

public sealed class FailTransactionCommandHandler(
    ITransactionRepository repository) : ICommandHandler<FailTransactionCommand, Result>
{
    public async Task<Result> Handle(FailTransactionCommand command, CancellationToken cancellationToken = default)
    {
        var transaction = await repository.GetByIdAsync(command.TransactionId, cancellationToken);
        if (transaction is null)
        {
            return Result.Failure(TransactionErrors.TransactionNotFound);
        }

        var result = transaction.Fail(command.FailureReason, DateTime.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
