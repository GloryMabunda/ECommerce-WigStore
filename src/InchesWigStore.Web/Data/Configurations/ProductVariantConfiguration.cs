using InchesWigStore.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InchesWigStore.Web.Data.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.Property(v => v.Sku).HasMaxLength(64);
        builder.Property(v => v.Texture).HasMaxLength(50);
        builder.Property(v => v.Colour).HasMaxLength(50);
        builder.Property(v => v.Density).HasMaxLength(50);
        builder.Property(v => v.CapSize).HasMaxLength(50);
        builder.Property(v => v.LaceType).HasMaxLength(50);
        builder.Property(v => v.Size).HasMaxLength(50);
        builder.Property(v => v.Style).HasMaxLength(50);
        builder.Property(v => v.RowVersion).IsRowVersion();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_ProductVariant_Stock_NonNegative", "[StockQuantity] >= 0");
            t.HasCheckConstraint("CK_ProductVariant_Price_NonNegative", "[PriceCents] >= 0");
        });

        // The default SQL Server collation is case-insensitive, and unique indexes treat NULLs as equal.
        builder.HasIndex(v => v.Sku).IsUnique();
        builder.HasIndex(v => new
        {
            v.ProductId,
            v.LengthInches,
            v.Texture,
            v.Colour,
            v.Density,
            v.CapSize,
            v.LaceType,
            v.Size,
            v.Style,
        }).IsUnique().HasFilter(null); // EF would otherwise exclude rows that have NULL options
        builder.HasIndex(v => v.LengthInches);
        builder.HasIndex(v => v.Texture);
        builder.HasIndex(v => v.Colour);
        builder.HasIndex(v => v.PriceCents);

        builder.HasOne(v => v.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
