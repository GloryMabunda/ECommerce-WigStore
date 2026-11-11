using InchesWigStore.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InchesWigStore.Web.Data.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.Property(a => a.RecipientName).HasMaxLength(200);
        builder.Property(a => a.Phone).HasMaxLength(30);
        builder.Property(a => a.Line1).HasMaxLength(200);
        builder.Property(a => a.Line2).HasMaxLength(200);
        builder.Property(a => a.City).HasMaxLength(100);
        builder.Property(a => a.Province).HasMaxLength(100);
        builder.Property(a => a.PostalCode).HasMaxLength(20);
        builder.HasIndex(a => a.UserId);
        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
