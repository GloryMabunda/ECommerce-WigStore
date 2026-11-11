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

    public static ApplicationUser User()
    {
        var id = Guid.NewGuid();
        return new ApplicationUser
        {
            Id = id,
            UserName = $"{id:N}@example.com",
            NormalizedUserName = $"{id:N}@EXAMPLE.COM",
            Email = $"{id:N}@example.com",
            NormalizedEmail = $"{id:N}@EXAMPLE.COM",
            DisplayName = "Test",
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>Builds a valid order (one unit of each variant) with a snapshot of the items and address.</summary>
    public static Order Order(ApplicationUser u, params ProductVariant[] v)
    {
        var n = Next();
        var items = v.Select(variant => new OrderItem
        {
            Variant = variant,
            ProductName = variant.Product.Name,
            Sku = variant.Sku,
            OptionsText = "Test options",
            Quantity = 1,
            UnitPriceCents = variant.PriceCents,
        }).ToList();
        var subtotal = items.Sum(i => i.UnitPriceCents * i.Quantity);
        const long delivery = 60_00;
        return new Order
        {
            Reference = $"IW-{n:D8}",
            User = u,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            PaymentMethod = PaymentMethod.Eft,
            SubtotalCents = subtotal,
            DeliveryFeeCents = delivery,
            TotalCents = subtotal + delivery,
            DeliveryAddress = new DeliveryAddressSnapshot
            {
                RecipientName = "Test Person",
                Phone = "0820000000",
                Line1 = "1 Test Street",
                City = "Johannesburg",
                Province = "Gauteng",
                PostalCode = "2000",
            },
            CreatedAt = DateTimeOffset.UtcNow,
            Items = items,
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
