using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Transaction.Domain.Errors;

namespace Vault.Services.Transaction.Application.Features.CompleteTransaction;

public sealed class CompleteTransactionCommandValidator : IRequestValidator<CompleteTransactionCommand>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(
        CompleteTransactionCommand request,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Error> errors = request.TransactionId == Guid.Empty
            ? [TransactionErrors.InvalidTransactionId]
            : Array.Empty<Error>();

        return Task.FromResult(errors);
    }
}
