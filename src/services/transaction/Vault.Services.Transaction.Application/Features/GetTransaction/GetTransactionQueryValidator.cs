using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Transaction.Domain.Errors;

namespace Vault.Services.Transaction.Application.Features.GetTransaction;

public sealed class GetTransactionQueryValidator : IRequestValidator<GetTransactionQuery>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(
        GetTransactionQuery request,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Error> errors = request.TransactionId == Guid.Empty
            ? [TransactionErrors.InvalidTransactionId]
            : Array.Empty<Error>();

        return Task.FromResult(errors);
    }
}
