namespace InchesWigStore.Web.Models;

public class PaymentNotification
{
    public int Id { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string NotificationId { get; set; } = string.Empty;

    public int? PaymentId { get; set; }

    public Payment? Payment { get; set; }

    public DateTimeOffset ReceivedAt { get; set; }

    public string Outcome { get; set; } = string.Empty;
}
