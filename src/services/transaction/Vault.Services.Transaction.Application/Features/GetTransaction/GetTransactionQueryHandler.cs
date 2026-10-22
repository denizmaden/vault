using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Transaction.Application.Abstractions.Persistence;
using Vault.Services.Transaction.Application.Models;
using Vault.Services.Transaction.Domain.Errors;

namespace Vault.Services.Transaction.Application.Features.GetTransaction;

public sealed class GetTransactionQueryHandler(
    ITransactionReadRepository repository) : IQueryHandler<GetTransactionQuery, Result<TransactionDetails>>
{
    public async Task<Result<TransactionDetails>> Handle(GetTransactionQuery query, CancellationToken cancellationToken = default)
    {
        var transaction = await repository.GetByIdAsync(query.TransactionId, cancellationToken);
        return transaction is null
            ? Result<TransactionDetails>.Failure(TransactionErrors.TransactionNotFound)
            : Result<TransactionDetails>.Success(transaction);
    }
}
