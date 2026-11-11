using InchesWigStore.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InchesWigStore.Web.Data.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.Property(m => m.Reason).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.ChangedBy).HasMaxLength(200);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_StockMovements_OldQuantity_NonNegative", "[OldQuantity] >= 0");
            t.HasCheckConstraint("CK_StockMovements_NewQuantity_NonNegative", "[NewQuantity] >= 0");
        });

        builder.HasIndex(m => new { m.VariantId, m.ChangedAt });
        builder.HasIndex(m => m.OrderId);
        builder.HasOne(m => m.Variant)
            .WithMany()
            .HasForeignKey(m => m.VariantId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Order)
            .WithMany()
            .HasForeignKey(m => m.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
