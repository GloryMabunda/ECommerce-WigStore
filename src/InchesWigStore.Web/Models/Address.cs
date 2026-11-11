namespace InchesWigStore.Web.Models;

public class Address
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public ApplicationUser? User { get; set; }

    public string RecipientName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Line1 { get; set; } = string.Empty;

    public string? Line2 { get; set; }

    public string City { get; set; } = string.Empty;

    public string Province { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
}
