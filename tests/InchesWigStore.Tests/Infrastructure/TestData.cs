using InchesWigStore.Web.Models;

namespace InchesWigStore.Tests.Infrastructure;

public static class TestData
{
    private static long _counter;

    private static long Next() => Interlocked.Increment(ref _counter);

    public static Category Category(string? slug = null)
    {
        var n = Next();
        return new Category
        {
            Name = $"Category {n}",
            Slug = slug ?? $"category-{n}",
            SortOrder = 0,
            IsActive = true,
        };
    }

    public static Product Product(Category c, string? slug = null)
    {
        var n = Next();
        return new Product
        {
            Category = c,
            Name = $"Product {n}",
            Slug = slug ?? $"product-{n}",
            Description = "Test product",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    public static ProductVariant Variant(Product p, string? sku = null, long priceCents = 100_00, int stock = 5)
    {
        var n = Next();
        return new ProductVariant
        {
            Product = p,
            Sku = sku ?? $"SKU-{n}",
            PriceCents = priceCents,
            StockQuantity = stock,
            IsActive = true,
        };
    }
}
