using InchesWigStore.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InchesWigStore.Web.Data.Configurations;

public class TryOnAssetConfiguration : IEntityTypeConfiguration<TryOnAsset>
{
    public void Configure(EntityTypeBuilder<TryOnAsset> builder)
    {
        builder.Property(a => a.Colour).HasMaxLength(50);
        builder.Property(a => a.StoragePath).HasMaxLength(500);
        builder.HasIndex(a => new { a.ProductId, a.Colour }).IsUnique();
        builder.HasOne(a => a.Product)
            .WithMany(p => p.TryOnAssets)
            .HasForeignKey(a => a.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
