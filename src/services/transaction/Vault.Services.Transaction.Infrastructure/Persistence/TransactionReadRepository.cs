using Microsoft.EntityFrameworkCore;
using Vault.Services.Transaction.Application.Abstractions.Persistence;
using Vault.Services.Transaction.Application.Models;

namespace Vault.Services.Transaction.Infrastructure.Persistence;

public sealed class TransactionReadRepository(TransactionDbContext dbContext) : ITransactionReadRepository
{
    public Task<TransactionDetails?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        return dbContext.Transactions
            .Where(transaction => transaction.Id == transactionId)
            .Select(transaction => new TransactionDetails(
                transaction.Id,
                transaction.InitiatingCustomerId,
                transaction.SourceWalletId,
                transaction.DestinationWalletId,
                transaction.Currency.Code,
                transaction.Amount.Value,
                transaction.Reference,
                transaction.Type.ToString(),
                transaction.Status.ToString(),
                transaction.FailureReason,
                transaction.CreatedOnUtc,
                transaction.LastUpdatedOnUtc,
                transaction.FundsReservedOnUtc,
                transaction.CompletedOnUtc,
                transaction.FailedOnUtc))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TransactionSummary>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Transactions
            .Where(transaction => transaction.InitiatingCustomerId == customerId)
            .OrderByDescending(transaction => transaction.CreatedOnUtc)
            .Select(transaction => new TransactionSummary(
                transaction.Id,
                transaction.InitiatingCustomerId,
                transaction.SourceWalletId,
                transaction.DestinationWalletId,
                transaction.Currency.Code,
                transaction.Amount.Value,
                transaction.Status.ToString(),
                transaction.CreatedOnUtc,
                transaction.LastUpdatedOnUtc))
            .ToArrayAsync(cancellationToken);
    }
}
