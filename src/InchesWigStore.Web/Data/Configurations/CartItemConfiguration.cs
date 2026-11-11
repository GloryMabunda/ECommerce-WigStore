using InchesWigStore.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InchesWigStore.Web.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable(t => t.HasCheckConstraint("CK_CartItems_Quantity_Positive", "[Quantity] > 0"));
        builder.HasIndex(i => new { i.CartId, i.VariantId })
            .IsUnique()
            .HasDatabaseName("IX_CartItems_CartId_VariantId");
        builder.HasIndex(i => i.VariantId);
        builder.HasOne(i => i.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(i => i.Variant)
            .WithMany()
            .HasForeignKey(i => i.VariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
