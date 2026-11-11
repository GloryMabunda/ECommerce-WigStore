using InchesWigStore.Tests.Infrastructure;
using InchesWigStore.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace InchesWigStore.Tests.Data;

public class CatalogueSchemaTests(SqlServerFixture fixture) : IClassFixture<SqlServerFixture>
{
    private static ProductVariant Wavy22(Product product, string? sku = null)
    {
        var variant = TestData.Variant(product, sku);
        variant.LengthInches = 22;
        variant.Texture = "Wavy";
        variant.Colour = "Black";
        return variant;
    }

    [Fact]
    public async Task Sku_is_unique_ignoring_case()
    {
        await using var context = fixture.CreateContext();
        var product = TestData.Product(TestData.Category());
        context.ProductVariants.Add(TestData.Variant(product, "wig-001"));
        await context.SaveChangesAsync();

        context.ProductVariants.Add(TestData.Variant(product, "WIG-001"));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task Duplicate_option_combination_on_same_product_is_refused()
    {
        await using var context = fixture.CreateContext();
        var category = TestData.Category();
        var product = TestData.Product(category);
        var other = TestData.Product(category);
        context.ProductVariants.Add(Wavy22(product));
        await context.SaveChangesAsync();

        context.ProductVariants.Add(Wavy22(other));
        await context.SaveChangesAsync();

        context.ProductVariants.Add(Wavy22(product));
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task Accessory_with_no_options_saves_once_but_second_identical_is_refused()
    {
        await using var context = fixture.CreateContext();
        var product = TestData.Product(TestData.Category());
        context.ProductVariants.Add(TestData.Variant(product));
        await context.SaveChangesAsync();

        context.ProductVariants.Add(TestData.Variant(product));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task Negative_stock_is_refused()
    {
        await using var context = fixture.CreateContext();
        context.ProductVariants.Add(TestData.Variant(TestData.Product(TestData.Category()), stock: -1));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task Negative_price_is_refused()
    {
        await using var context = fixture.CreateContext();
        context.ProductVariants.Add(TestData.Variant(TestData.Product(TestData.Category()), priceCents: -1));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task Zero_stock_and_zero_price_are_allowed()
    {
        await using var context = fixture.CreateContext();
        context.ProductVariants.Add(
            TestData.Variant(TestData.Product(TestData.Category()), priceCents: 0, stock: 0));

        var saved = await context.SaveChangesAsync();

        Assert.True(saved > 0);
    }

    [Fact]
    public async Task Concurrent_stock_updates_raise_a_concurrency_error()
    {
        int id;
        await using (var seed = fixture.CreateContext())
        {
            var variant = TestData.Variant(TestData.Product(TestData.Category()), stock: 5);
            seed.ProductVariants.Add(variant);
            await seed.SaveChangesAsync();
            id = variant.Id;
        }

        await using var first = fixture.CreateContext();
        await using var second = fixture.CreateContext();
        var a = await first.ProductVariants.SingleAsync(v => v.Id == id);
        var b = await second.ProductVariants.SingleAsync(v => v.Id == id);

        a.StockQuantity = 4;
        b.StockQuantity = 3;
        await first.SaveChangesAsync();

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
    }

    [Fact]
    public async Task Duplicate_category_slug_and_product_slug_are_refused()
    {
        await using var context = fixture.CreateContext();
        var category = TestData.Category("dup-category");
        var product = TestData.Product(category, "dup-product");
        context.Products.Add(product);
        await context.SaveChangesAsync();

        context.Categories.Add(TestData.Category("DUP-CATEGORY"));
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        context.ChangeTracker.Clear();

        context.Products.Add(TestData.Product(await context.Categories.FirstAsync(c => c.Slug == "dup-category"), "dup-product"));
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task TryOnAsset_is_unique_per_product_and_colour()
    {
        await using var context = fixture.CreateContext();
        var product = TestData.Product(TestData.Category());
        var other = TestData.Product(product.Category);
        context.TryOnAssets.Add(NewAsset(product, "Black"));
        await context.SaveChangesAsync();

        context.TryOnAssets.Add(NewAsset(product, "Brown"));
        context.TryOnAssets.Add(NewAsset(other, "Black"));
        await context.SaveChangesAsync();

        context.TryOnAssets.Add(NewAsset(product, "Black"));
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task Variant_with_a_product_cannot_outlive_it()
    {
        int productId;
        await using (var seed = fixture.CreateContext())
        {
            var variant = TestData.Variant(TestData.Product(TestData.Category()));
            seed.ProductVariants.Add(variant);
            await seed.SaveChangesAsync();
            productId = variant.ProductId;
        }

        // A fresh context does not track the variants, so the refusal must come from the database foreign key.
        await using var context = fixture.CreateContext();
        context.Products.Remove(await context.Products.SingleAsync(p => p.Id == productId));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    private static TryOnAsset NewAsset(Product product, string colour) => new()
    {
        Product = product,
        Colour = colour,
        StoragePath = $"tryon/{colour}.png",
        OpeningX = 0.1,
        OpeningY = 0.2,
        OpeningWidth = 0.5,
        OpeningHeight = 0.5,
    };
}
