using Microsoft.EntityFrameworkCore;
using Vault.Services.Transaction.Application.Abstractions.Persistence;
using Vault.Services.Transaction.Domain.Entities;

namespace Vault.Services.Transaction.Infrastructure.Persistence;

public sealed class TransactionRepository(TransactionDbContext dbContext) : ITransactionRepository
{
    public Task<bool> ExistsAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        return dbContext.Transactions.AnyAsync(transaction => transaction.Id == transactionId, cancellationToken);
    }

    public Task AddAsync(TransactionRecord transaction, CancellationToken cancellationToken = default)
    {
        return dbContext.Transactions.AddAsync(transaction, cancellationToken).AsTask();
    }

    public Task<TransactionRecord?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        return dbContext.Transactions.SingleOrDefaultAsync(transaction => transaction.Id == transactionId, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
