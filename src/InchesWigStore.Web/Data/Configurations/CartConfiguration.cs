using InchesWigStore.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InchesWigStore.Web.Data.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.Property(c => c.VisitorToken).HasMaxLength(64);
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Carts_Exactly_One_Owner",
            "([UserId] IS NOT NULL AND [VisitorToken] IS NULL) OR ([UserId] IS NULL AND [VisitorToken] IS NOT NULL)"));
        // Filtered so any number of visitor carts (UserId NULL) are allowed while each user has at most one cart.
        builder.HasIndex(c => c.UserId)
            .IsUnique()
            .HasDatabaseName("IX_Carts_UserId")
            .HasFilter("[UserId] IS NOT NULL");
        builder.HasIndex(c => c.VisitorToken);
        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
