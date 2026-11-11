namespace InchesWigStore.Web.Models;

public class Cart
{
    public int Id { get; set; }

    public Guid? UserId { get; set; }

    public ApplicationUser? User { get; set; }

    public string? VisitorToken { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public List<CartItem> Items { get; set; } = [];
}
