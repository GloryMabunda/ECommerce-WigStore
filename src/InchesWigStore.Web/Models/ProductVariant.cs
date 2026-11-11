namespace InchesWigStore.Web.Models;

public class ProductVariant
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public string Sku { get; set; } = string.Empty;

    public long PriceCents { get; set; }

    public int StockQuantity { get; set; }

    public bool IsActive { get; set; }

    public int? LengthInches { get; set; }

    public string? Texture { get; set; }

    public string? Colour { get; set; }

    public string? Density { get; set; }

    public string? CapSize { get; set; }

    public string? LaceType { get; set; }

    public string? Size { get; set; }

    public string? Style { get; set; }

    public byte[] RowVersion { get; set; } = [];
}
