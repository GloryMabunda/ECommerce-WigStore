namespace InchesWigStore.Web.Models;

public class OrderStatusHistory
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;

    /// <summary>Either "Order" or "Payment".</summary>
    public string Kind { get; set; } = string.Empty;

    public string FromStatus { get; set; } = string.Empty;

    public string ToStatus { get; set; } = string.Empty;

    public string ChangedBy { get; set; } = string.Empty;

    public DateTimeOffset ChangedAt { get; set; }
}
