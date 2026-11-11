using InchesWigStore.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InchesWigStore.Web.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(o => o.Reference).HasMaxLength(20);
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.PaymentStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.PaymentMethod).HasConversion<string>().HasMaxLength(20);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_Orders_Total_Equals_Subtotal_Plus_Delivery",
                "[TotalCents] = [SubtotalCents] + [DeliveryFeeCents]");
            t.HasCheckConstraint("CK_Orders_Subtotal_NonNegative", "[SubtotalCents] >= 0");
            t.HasCheckConstraint("CK_Orders_DeliveryFee_NonNegative", "[DeliveryFeeCents] >= 0");
            t.HasCheckConstraint("CK_Orders_Total_NonNegative", "[TotalCents] >= 0");
        });

        builder.OwnsOne(o => o.DeliveryAddress, a =>
        {
            a.Property(x => x.RecipientName).HasColumnName("ShipRecipientName").HasMaxLength(200);
            a.Property(x => x.Phone).HasColumnName("ShipPhone").HasMaxLength(30);
            a.Property(x => x.Line1).HasColumnName("ShipLine1").HasMaxLength(200);
            a.Property(x => x.Line2).HasColumnName("ShipLine2").HasMaxLength(200);
            a.Property(x => x.City).HasColumnName("ShipCity").HasMaxLength(100);
            a.Property(x => x.Province).HasColumnName("ShipProvince").HasMaxLength(100);
            a.Property(x => x.PostalCode).HasColumnName("ShipPostalCode").HasMaxLength(20);
        });
        builder.Navigation(o => o.DeliveryAddress).IsRequired();

        builder.HasIndex(o => o.Reference).IsUnique().HasDatabaseName("IX_Orders_Reference");
        builder.HasIndex(o => o.UserId);
        builder.HasIndex(o => o.CreatedAt);
        builder.HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
