namespace InchesWigStore.Web.Models;

public class StockMovement
{
    public int Id { get; set; }

    public int VariantId { get; set; }

    public ProductVariant Variant { get; set; } = null!;

    public int OldQuantity { get; set; }

    public int NewQuantity { get; set; }

    public StockMovementReason Reason { get; set; }

    public string ChangedBy { get; set; } = string.Empty;

    public int? OrderId { get; set; }

    public Order? Order { get; set; }

    public DateTimeOffset ChangedAt { get; set; }
}
