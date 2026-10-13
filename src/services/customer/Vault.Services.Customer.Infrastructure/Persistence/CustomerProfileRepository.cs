using Microsoft.EntityFrameworkCore;
using Vault.Services.Customer.Application.Abstractions.Persistence;
using Vault.Services.Customer.Domain.Entities;

namespace Vault.Services.Customer.Infrastructure.Persistence;

public sealed class CustomerProfileRepository(CustomerDbContext dbContext) : ICustomerProfileRepository
{
    public Task<bool> ExistsForIdentityUserAsync(Guid identityUserId, CancellationToken cancellationToken = default)
    {
        return dbContext.CustomerProfiles.AnyAsync(
            profile => profile.IdentityUserId == identityUserId,
            cancellationToken);
    }

    public async Task AddAsync(CustomerProfile customerProfile, CancellationToken cancellationToken = default)
    {
        await dbContext.CustomerProfiles.AddAsync(customerProfile, cancellationToken);
    }

    public Task<CustomerProfile?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return dbContext.CustomerProfiles.FirstOrDefaultAsync(
            profile => profile.Id == customerId,
            cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
