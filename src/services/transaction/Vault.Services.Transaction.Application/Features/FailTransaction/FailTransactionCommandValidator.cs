using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Transaction.Domain.Errors;

namespace Vault.Services.Transaction.Application.Features.FailTransaction;

public sealed class FailTransactionCommandValidator : IRequestValidator<FailTransactionCommand>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(
        FailTransactionCommand request,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<Error>();

        if (request.TransactionId == Guid.Empty)
        {
            errors.Add(TransactionErrors.InvalidTransactionId);
        }

        if (request.FailureReason is { Length: > 250 })
        {
            errors.Add(TransactionErrors.InvalidFailureReason);
        }

        return Task.FromResult<IReadOnlyCollection<Error>>(errors);
    }
}
