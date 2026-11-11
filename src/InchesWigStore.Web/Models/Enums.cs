namespace InchesWigStore.Web.Models;

public enum OrderStatus
{
    Pending,
    Confirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
}

public enum PaymentStatus
{
    Pending,
    Paid,
    Failed,
    Refunded,
}

public enum PaymentMethod
{
    Eft,
    Online,
}
