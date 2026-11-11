namespace InchesWigStore.Web.Models;

public class CartItem
{
    public int Id { get; set; }

    public int CartId { get; set; }

    public Cart Cart { get; set; } = null!;

    public int VariantId { get; set; }

    public ProductVariant Variant { get; set; } = null!;

    public int Quantity { get; set; }
}
