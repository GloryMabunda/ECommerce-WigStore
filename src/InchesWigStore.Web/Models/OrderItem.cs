namespace InchesWigStore.Web.Models;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;

    public int VariantId { get; set; }

    public ProductVariant Variant { get; set; } = null!;

    public string ProductName { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public string OptionsText { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public long UnitPriceCents { get; set; }
}
