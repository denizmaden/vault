using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vault.Services.Customer.Domain.Entities;

namespace Vault.Services.Customer.Infrastructure.Persistence.Configurations;

public sealed class CustomerProfileConfiguration : IEntityTypeConfiguration<CustomerProfile>
{
    public void Configure(EntityTypeBuilder<CustomerProfile> builder)
    {
        builder.ToTable("CustomerProfiles");
        builder.HasKey(profile => profile.Id);

        builder.Property(profile => profile.IdentityUserId)
            .IsRequired();

        builder.HasIndex(profile => profile.IdentityUserId)
            .IsUnique();

        builder.Property(profile => profile.CreatedOnUtc)
            .IsRequired();

        builder.Property(profile => profile.OnboardingStatus)
            .IsRequired();

        builder.Property(profile => profile.OnboardingCompletedOnUtc);

        builder.OwnsOne(profile => profile.LegalName, ownedBuilder =>
        {
            ownedBuilder.Property(name => name.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(100)
                .IsRequired();

            ownedBuilder.Property(name => name.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.OwnsOne(profile => profile.DisplayName, ownedBuilder =>
        {
            ownedBuilder.Property(displayName => displayName.Value)
                .HasColumnName("DisplayName")
                .HasMaxLength(150)
                .IsRequired();
        });

        builder.OwnsOne(profile => profile.PreferredLanguage, ownedBuilder =>
        {
            ownedBuilder.Property(language => language.Value)
                .HasColumnName("PreferredLanguage")
                .HasMaxLength(10)
                .IsRequired();
        });
    }
}
