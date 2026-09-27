using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vault.Services.Identity.Infrastructure.Identity;

namespace Vault.Services.Identity.Infrastructure.Persistence.Configurations;

public sealed class IdentityUserConfiguration : IEntityTypeConfiguration<VaultIdentityUser>
{
    public void Configure(EntityTypeBuilder<VaultIdentityUser> builder)
    {
        builder.ToTable("IdentityUsers");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.UserName)
            .HasMaxLength(256);

        builder.Property(user => user.NormalizedUserName)
            .HasMaxLength(256);

        builder.Property(user => user.Email)
            .HasMaxLength(256);

        builder.Property(user => user.NormalizedEmail)
            .HasMaxLength(256);

        builder.Property(user => user.CreatedOnUtc)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique()
            .HasDatabaseName("IX_IdentityUsers_Email");
    }
}
