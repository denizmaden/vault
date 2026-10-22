using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vault.Services.Ledger.Infrastructure.Persistence.Documents;

namespace Vault.Services.Ledger.Infrastructure.Persistence.Configurations;

public sealed class LedgerAccountProjectionConfiguration : IEntityTypeConfiguration<LedgerAccountProjection>
{
    public void Configure(EntityTypeBuilder<LedgerAccountProjection> builder)
    {
        builder.ToTable("LedgerAccounts");
        builder.HasKey(x => x.LedgerAccountId);
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Balance).HasPrecision(18, 2);
        builder.Property(x => x.Version).IsRequired();
        builder.Property(x => x.OpenedOnUtc).IsRequired();
        builder.Property(x => x.LastUpdatedOnUtc).IsRequired();

        builder.HasIndex(x => x.OwnerId);
    }
}

