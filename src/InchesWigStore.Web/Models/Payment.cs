namespace InchesWigStore.Web.Models;

public class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;

    public PaymentMethod Method { get; set; }

    public PaymentStatus Status { get; set; }

    public long AmountCents { get; set; }

    public long? AmountReceivedCents { get; set; }

    public string Reference { get; set; } = string.Empty;

    public string? ProviderReference { get; set; }

    public string? VerifiedBy { get; set; }

    public DateTimeOffset? VerifiedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
