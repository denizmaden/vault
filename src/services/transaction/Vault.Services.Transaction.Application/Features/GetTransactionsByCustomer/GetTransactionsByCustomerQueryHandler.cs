using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Transaction.Application.Abstractions.Persistence;
using Vault.Services.Transaction.Application.Models;

namespace Vault.Services.Transaction.Application.Features.GetTransactionsByCustomer;

public sealed class GetTransactionsByCustomerQueryHandler(
    ITransactionReadRepository repository) : IQueryHandler<GetTransactionsByCustomerQuery, Result<IReadOnlyCollection<TransactionSummary>>>
{
    public async Task<Result<IReadOnlyCollection<TransactionSummary>>> Handle(
        GetTransactionsByCustomerQuery query,
        CancellationToken cancellationToken = default)
    {
        var transactions = await repository.GetByCustomerAsync(query.CustomerId, cancellationToken);
        return Result<IReadOnlyCollection<TransactionSummary>>.Success(transactions);
    }
}
