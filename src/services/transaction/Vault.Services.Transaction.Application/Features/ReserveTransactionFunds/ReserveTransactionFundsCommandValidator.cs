using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Transaction.Domain.Errors;

namespace Vault.Services.Transaction.Application.Features.ReserveTransactionFunds;

public sealed class ReserveTransactionFundsCommandValidator : IRequestValidator<ReserveTransactionFundsCommand>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(
        ReserveTransactionFundsCommand request,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Error> errors = request.TransactionId == Guid.Empty
            ? [TransactionErrors.InvalidTransactionId]
            : Array.Empty<Error>();

        return Task.FromResult(errors);
    }
}
