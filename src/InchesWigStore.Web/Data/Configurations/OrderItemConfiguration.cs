using InchesWigStore.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InchesWigStore.Web.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.Property(i => i.ProductName).HasMaxLength(200);
        builder.Property(i => i.Sku).HasMaxLength(64);
        builder.Property(i => i.OptionsText).HasMaxLength(500);
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_OrderItems_Quantity_Positive", "[Quantity] > 0");
            t.HasCheckConstraint("CK_OrderItems_UnitPrice_NonNegative", "[UnitPriceCents] >= 0");
        });
        builder.HasIndex(i => i.OrderId);
        builder.HasIndex(i => i.VariantId);
        builder.HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(i => i.Variant)
            .WithMany()
            .HasForeignKey(i => i.VariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
