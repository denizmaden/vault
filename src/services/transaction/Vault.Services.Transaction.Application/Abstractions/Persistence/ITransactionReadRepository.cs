using Vault.Services.Transaction.Application.Models;

namespace Vault.Services.Transaction.Application.Abstractions.Persistence;

public interface ITransactionReadRepository
{
    Task<TransactionDetails?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TransactionSummary>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
}
