using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Identity.Application.Abstractions.Identity;

public interface IIdentityUserRegistrationService
{
    Task<Result<Guid>> RegisterAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);
}
