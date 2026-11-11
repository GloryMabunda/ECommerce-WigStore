namespace InchesWigStore.Web.Models;

public class TryOnAsset
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public string Colour { get; set; } = string.Empty;

    public string StoragePath { get; set; } = string.Empty;

    public double OpeningX { get; set; }

    public double OpeningY { get; set; }

    public double OpeningWidth { get; set; }

    public double OpeningHeight { get; set; }
}
