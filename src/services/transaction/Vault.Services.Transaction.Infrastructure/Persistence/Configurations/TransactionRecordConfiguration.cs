using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vault.Services.Transaction.Domain.Entities;

namespace Vault.Services.Transaction.Infrastructure.Persistence.Configurations;

public sealed class TransactionRecordConfiguration : IEntityTypeConfiguration<TransactionRecord>
{
    public void Configure(EntityTypeBuilder<TransactionRecord> builder)
    {
        builder.ToTable("Transactions");
        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.InitiatingCustomerId).IsRequired();
        builder.Property(transaction => transaction.SourceWalletId).IsRequired();
        builder.Property(transaction => transaction.DestinationWalletId).IsRequired();
        builder.Property(transaction => transaction.Type).IsRequired();
        builder.Property(transaction => transaction.Status).IsRequired();
        builder.Property(transaction => transaction.Reference).HasMaxLength(150).IsRequired();
        builder.Property(transaction => transaction.FailureReason).HasMaxLength(250);
        builder.Property(transaction => transaction.CreatedOnUtc).IsRequired();
        builder.Property(transaction => transaction.LastUpdatedOnUtc).IsRequired();
        builder.Property(transaction => transaction.FundsReservedOnUtc);
        builder.Property(transaction => transaction.CompletedOnUtc);
        builder.Property(transaction => transaction.FailedOnUtc);

        builder.HasIndex(transaction => transaction.InitiatingCustomerId);
        builder.HasIndex(transaction => transaction.Status);

        builder.OwnsOne(transaction => transaction.Currency, ownedBuilder =>
        {
            ownedBuilder.Property(currency => currency.Code)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(transaction => transaction.Amount, ownedBuilder =>
        {
            ownedBuilder.Property(amount => amount.Value)
                .HasColumnName("Amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
        });
    }
}
