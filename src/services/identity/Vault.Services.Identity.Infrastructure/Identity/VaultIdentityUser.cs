using Microsoft.AspNetCore.Identity;

namespace Vault.Services.Identity.Infrastructure.Identity;

public sealed class VaultIdentityUser : IdentityUser<Guid>
{
    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
}
