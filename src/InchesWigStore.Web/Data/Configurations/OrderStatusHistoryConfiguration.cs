using InchesWigStore.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InchesWigStore.Web.Data.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.Property(h => h.Kind).HasMaxLength(10);
        builder.Property(h => h.FromStatus).HasMaxLength(20);
        builder.Property(h => h.ToStatus).HasMaxLength(20);
        builder.Property(h => h.ChangedBy).HasMaxLength(200);
        builder.HasIndex(h => new { h.OrderId, h.ChangedAt });
        builder.HasOne(h => h.Order)
            .WithMany(o => o.History)
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
