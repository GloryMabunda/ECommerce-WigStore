namespace InchesWigStore.Web.Models;

public class DeliveryAddressSnapshot
{
    public string RecipientName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Line1 { get; set; } = string.Empty;

    public string? Line2 { get; set; }

    public string City { get; set; } = string.Empty;

    public string Province { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;
}
