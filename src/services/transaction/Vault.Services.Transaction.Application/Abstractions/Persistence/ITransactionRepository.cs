using Vault.Services.Transaction.Domain.Entities;

namespace Vault.Services.Transaction.Application.Abstractions.Persistence;

public interface ITransactionRepository
{
    Task<bool> ExistsAsync(Guid transactionId, CancellationToken cancellationToken = default);

    Task AddAsync(TransactionRecord transaction, CancellationToken cancellationToken = default);

    Task<TransactionRecord?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
