namespace InchesWigStore.Web.Models;

public class Product
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<ProductVariant> Variants { get; } = [];

    public ICollection<ProductImage> Images { get; } = [];

    public ICollection<TryOnAsset> TryOnAssets { get; } = [];
}
