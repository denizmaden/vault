using Microsoft.EntityFrameworkCore;

namespace Vault.Platform.Persistence.EntityFramework.Contexts;

public abstract class VaultDbContext(DbContextOptions options) : DbContext(options);
