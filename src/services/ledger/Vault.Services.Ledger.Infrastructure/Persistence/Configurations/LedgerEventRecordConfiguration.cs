using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vault.Services.Ledger.Infrastructure.Persistence.Documents;

namespace Vault.Services.Ledger.Infrastructure.Persistence.Configurations;

public sealed class LedgerEventRecordConfiguration : IEntityTypeConfiguration<LedgerEventRecord>
{
    public void Configure(EntityTypeBuilder<LedgerEventRecord> builder)
    {
        builder.ToTable("LedgerEvents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Data).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.Version).IsRequired();
        builder.Property(x => x.OccurredOnUtc).IsRequired();

        builder.HasIndex(x => new { x.LedgerAccountId, x.Version }).IsUnique();
        builder.HasIndex(x => x.LedgerAccountId);
    }
}

