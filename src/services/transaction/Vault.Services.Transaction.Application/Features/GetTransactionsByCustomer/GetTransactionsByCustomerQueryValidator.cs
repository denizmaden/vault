using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Transaction.Domain.Errors;

namespace Vault.Services.Transaction.Application.Features.GetTransactionsByCustomer;

public sealed class GetTransactionsByCustomerQueryValidator : IRequestValidator<GetTransactionsByCustomerQuery>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(
        GetTransactionsByCustomerQuery request,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Error> errors = request.CustomerId == Guid.Empty
            ? [TransactionErrors.InvalidCustomerId]
            : Array.Empty<Error>();

        return Task.FromResult(errors);
    }
}
