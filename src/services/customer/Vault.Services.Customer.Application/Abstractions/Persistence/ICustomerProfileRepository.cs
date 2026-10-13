using Vault.Services.Customer.Domain.Entities;

namespace Vault.Services.Customer.Application.Abstractions.Persistence;

public interface ICustomerProfileRepository
{
    Task<bool> ExistsForIdentityUserAsync(Guid identityUserId, CancellationToken cancellationToken = default);

    Task AddAsync(CustomerProfile customerProfile, CancellationToken cancellationToken = default);

    Task<CustomerProfile?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
