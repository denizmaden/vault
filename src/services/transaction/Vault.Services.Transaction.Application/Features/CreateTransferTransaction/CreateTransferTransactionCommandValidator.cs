using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Transaction.Domain.Errors;

namespace Vault.Services.Transaction.Application.Features.CreateTransferTransaction;

public sealed class CreateTransferTransactionCommandValidator : IRequestValidator<CreateTransferTransactionCommand>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(
        CreateTransferTransactionCommand request,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<Error>();

        if (request.TransactionId == Guid.Empty)
        {
            errors.Add(TransactionErrors.InvalidTransactionId);
        }

        if (request.InitiatingCustomerId == Guid.Empty)
        {
            errors.Add(TransactionErrors.InvalidCustomerId);
        }

        if (request.SourceWalletId == Guid.Empty || request.DestinationWalletId == Guid.Empty)
        {
            errors.Add(TransactionErrors.InvalidWalletId);
        }

        if (request.SourceWalletId == request.DestinationWalletId)
        {
            errors.Add(TransactionErrors.DuplicateWallets);
        }

        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            errors.Add(TransactionErrors.InvalidCurrency);
        }

        if (request.Amount <= 0)
        {
            errors.Add(TransactionErrors.InvalidAmount);
        }

        return Task.FromResult<IReadOnlyCollection<Error>>(errors);
    }
}
