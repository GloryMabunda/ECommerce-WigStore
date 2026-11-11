using InchesWigStore.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InchesWigStore.Web.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.Property(p => p.Method).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Reference).HasMaxLength(64);
        builder.Property(p => p.ProviderReference).HasMaxLength(100);
        builder.Property(p => p.VerifiedBy).HasMaxLength(200);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Payments_Amount_NonNegative", "[AmountCents] >= 0");
            t.HasCheckConstraint(
                "CK_Payments_AmountReceived_NonNegative",
                "[AmountReceivedCents] IS NULL OR [AmountReceivedCents] >= 0");
        });

        builder.HasIndex(p => p.Reference).IsUnique().HasDatabaseName("IX_Payments_Reference");
        // Filtered so payments not yet reported by the provider (NULL reference) do not collide with each other.
        builder.HasIndex(p => p.ProviderReference)
            .IsUnique()
            .HasDatabaseName("IX_Payments_ProviderReference")
            .HasFilter("[ProviderReference] IS NOT NULL");
        builder.HasIndex(p => p.OrderId);
        builder.HasOne(p => p.Order)
            .WithMany()
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
