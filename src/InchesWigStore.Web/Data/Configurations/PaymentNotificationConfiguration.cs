using InchesWigStore.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InchesWigStore.Web.Data.Configurations;

public class PaymentNotificationConfiguration : IEntityTypeConfiguration<PaymentNotification>
{
    public void Configure(EntityTypeBuilder<PaymentNotification> builder)
    {
        builder.Property(n => n.Provider).HasMaxLength(50);
        builder.Property(n => n.NotificationId).HasMaxLength(200);
        builder.Property(n => n.Outcome).HasMaxLength(50);

        builder.HasIndex(n => new { n.Provider, n.NotificationId })
            .IsUnique()
            .HasDatabaseName("IX_PaymentNotifications_Provider_NotificationId");
        builder.HasIndex(n => n.PaymentId);
        builder.HasOne(n => n.Payment)
            .WithMany()
            .HasForeignKey(n => n.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
