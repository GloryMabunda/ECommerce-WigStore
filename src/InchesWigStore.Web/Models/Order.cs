namespace InchesWigStore.Web.Models;

public class Order
{
    public int Id { get; set; }

    public string Reference { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public OrderStatus Status { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public long SubtotalCents { get; set; }

    public long DeliveryFeeCents { get; set; }

    public long TotalCents { get; set; }

    public DeliveryAddressSnapshot DeliveryAddress { get; set; } = new();

    public DateTimeOffset CreatedAt { get; set; }

    public List<OrderItem> Items { get; set; } = [];

    public List<OrderStatusHistory> History { get; set; } = [];
}
