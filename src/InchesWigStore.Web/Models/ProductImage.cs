namespace InchesWigStore.Web.Models;

public class ProductImage
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public string StoragePath { get; set; } = string.Empty;

    public string AltText { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsMain { get; set; }
}
