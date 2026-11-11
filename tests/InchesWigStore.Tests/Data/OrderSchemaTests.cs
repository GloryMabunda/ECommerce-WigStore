using InchesWigStore.Web.Data;
using InchesWigStore.Tests.Infrastructure;
using InchesWigStore.Web.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace InchesWigStore.Tests.Data;

public class OrderSchemaTests(SqlServerFixture fixture) : IClassFixture<SqlServerFixture>
{
    private static SqlException Inner(DbUpdateException ex) =>
        Assert.IsType<SqlException>(ex.InnerException);

    private static async Task<(Order Order, ApplicationUser User, ProductVariant Variant)> SaveOrderAsync(AppDbContext context)
    {
        var user = TestData.User();
        var variant = TestData.Variant(TestData.Product(TestData.Category()));
        var order = TestData.Order(user, variant);
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        return (order, user, variant);
    }

    private static async Task<SqlException> AssertRefusedAsync(AppDbContext context, int number, string constraint)
    {
        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        var sql = Inner(ex);
        Assert.Equal(number, sql.Number);
        Assert.Contains(constraint, sql.Message);
        return sql;
    }

    [Fact]
    public async Task Order_keeps_its_snapshot_when_variant_and_product_change()
    {
        await using var context = fixture.CreateContext();
        var (order, _, variant) = await SaveOrderAsync(context);
        var originalName = variant.Product.Name;
        var originalSku = variant.Sku;
        var originalPrice = variant.PriceCents;

        await using (var change = fixture.CreateContext())
        {
            var v = await change.ProductVariants.Include(x => x.Product).SingleAsync(x => x.Id == variant.Id);
            v.PriceCents += 5_00;
            v.Product.Name = "Renamed product";
            await change.SaveChangesAsync();
        }

        await using var read = fixture.CreateContext();
        var item = (await read.Orders.Include(o => o.Items).SingleAsync(o => o.Id == order.Id)).Items.Single();
        Assert.Equal(originalName, item.ProductName);
        Assert.Equal(originalSku, item.Sku);
        Assert.Equal(originalPrice, item.UnitPriceCents);
    }

    [Fact]
    public async Task Order_total_must_equal_subtotal_plus_delivery_fee()
    {
        await using var context = fixture.CreateContext();
        var order = TestData.Order(TestData.User(), TestData.Variant(TestData.Product(TestData.Category())));
        order.TotalCents += 1;
        context.Orders.Add(order);

        await AssertRefusedAsync(context, 547, "CK_Orders_Total_Equals_Subtotal_Plus_Delivery");
    }

    [Fact]
    public async Task Order_reference_is_unique()
    {
        await using var context = fixture.CreateContext();
        var (first, user, variant) = await SaveOrderAsync(context);

        await using var other = fixture.CreateContext();
        var duplicate = TestData.Order(TestData.User(), TestData.Variant(TestData.Product(TestData.Category())));
        duplicate.Reference = first.Reference;
        other.Orders.Add(duplicate);

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => other.SaveChangesAsync());
        var sql = Inner(ex);
        Assert.True(sql.Number is 2601 or 2627, $"Unexpected SQL error {sql.Number}");
        Assert.Contains("IX_Orders_Reference", sql.Message);
        Assert.NotNull(user);
        Assert.NotNull(variant);
    }

    [Fact]
    public async Task Order_item_quantity_zero_or_negative_is_refused()
    {
        foreach (var quantity in new[] { 0, -1 })
        {
            await using var context = fixture.CreateContext();
            var order = TestData.Order(TestData.User(), TestData.Variant(TestData.Product(TestData.Category())));
            order.Items[0].Quantity = quantity;
            context.Orders.Add(order);

            await AssertRefusedAsync(context, 547, "CK_OrderItems_Quantity_Positive");
        }
    }

    [Fact]
    public async Task Cart_item_quantity_zero_is_refused()
    {
        await using var context = fixture.CreateContext();
        var cart = new Cart { VisitorToken = Guid.NewGuid().ToString("N"), UpdatedAt = DateTimeOffset.UtcNow };
        cart.Items.Add(new CartItem
        {
            Variant = TestData.Variant(TestData.Product(TestData.Category())),
            Quantity = 0,
        });
        context.Carts.Add(cart);

        await AssertRefusedAsync(context, 547, "CK_CartItems_Quantity_Positive");
    }

    [Fact]
    public async Task Variant_that_is_on_an_order_cannot_be_deleted()
    {
        await using var context = fixture.CreateContext();
        var (_, _, variant) = await SaveOrderAsync(context);

        await using var other = fixture.CreateContext();
        other.ProductVariants.Remove(await other.ProductVariants.SingleAsync(v => v.Id == variant.Id));

        await AssertRefusedAsync(other, 547, "FK_OrderItems_ProductVariants_VariantId");
    }

    [Fact]
    public async Task Product_that_is_on_an_order_cannot_be_deleted()
    {
        await using var context = fixture.CreateContext();
        var (_, _, variant) = await SaveOrderAsync(context);

        await using var other = fixture.CreateContext();
        var loaded = await other.ProductVariants.Include(v => v.Product).SingleAsync(v => v.Id == variant.Id);
        other.ProductVariants.Remove(loaded);
        other.Products.Remove(loaded.Product);

        await AssertRefusedAsync(other, 547, "FK_OrderItems_ProductVariants_VariantId");
    }

    [Fact]
    public async Task User_with_an_order_cannot_be_deleted()
    {
        await using var context = fixture.CreateContext();
        var (_, user, _) = await SaveOrderAsync(context);

        await using var other = fixture.CreateContext();
        other.Users.Remove(await other.Users.SingleAsync(u => u.Id == user.Id));

        await AssertRefusedAsync(other, 547, "FK_Orders_AspNetUsers_UserId");
    }

    [Fact]
    public async Task Order_and_payment_statuses_change_independently_and_are_stored_as_text()
    {
        await using var context = fixture.CreateContext();
        var (order, _, _) = await SaveOrderAsync(context);

        await using (var change = fixture.CreateContext())
        {
            var o = await change.Orders.SingleAsync(x => x.Id == order.Id);
            o.Status = OrderStatus.Shipped;
            o.PaymentStatus = PaymentStatus.Paid;
            await change.SaveChangesAsync();
        }

        await using var read = fixture.CreateContext();
        var reloaded = await read.Orders.SingleAsync(x => x.Id == order.Id);
        Assert.Equal(OrderStatus.Shipped, reloaded.Status);
        Assert.Equal(PaymentStatus.Paid, reloaded.PaymentStatus);
        Assert.Equal(PaymentMethod.Eft, reloaded.PaymentMethod);

        var raw = await read.Database
            .SqlQuery<string>($"SELECT [Status] AS [Value] FROM [Orders] WHERE [Id] = {order.Id}")
            .SingleAsync();
        Assert.Equal("Shipped", raw);
    }

    [Fact]
    public async Task Status_history_rows_are_kept_in_order()
    {
        await using var context = fixture.CreateContext();
        var (order, _, _) = await SaveOrderAsync(context);
        var start = DateTimeOffset.UtcNow;

        await using (var change = fixture.CreateContext())
        {
            // Added out of chronological order on purpose.
            change.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                Kind = "Order",
                FromStatus = "Confirmed",
                ToStatus = "Shipped",
                ChangedBy = "admin@example.com",
                ChangedAt = start.AddMinutes(2),
            });
            change.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                Kind = "Order",
                FromStatus = "Pending",
                ToStatus = "Confirmed",
                ChangedBy = "admin@example.com",
                ChangedAt = start.AddMinutes(1),
            });
            await change.SaveChangesAsync();
        }

        await using var read = fixture.CreateContext();
        var rows = await read.OrderStatusHistories
            .Where(h => h.OrderId == order.Id)
            .OrderBy(h => h.ChangedAt)
            .ToListAsync();
        Assert.Equal(["Pending", "Confirmed"], rows.Select(r => r.FromStatus));
        Assert.Equal(["Confirmed", "Shipped"], rows.Select(r => r.ToStatus));
    }

    [Fact]
    public async Task Deleting_an_order_removes_its_items_but_not_the_variant()
    {
        await using var context = fixture.CreateContext();
        var (order, _, variant) = await SaveOrderAsync(context);

        await using (var change = fixture.CreateContext())
        {
            change.Orders.Remove(await change.Orders.SingleAsync(o => o.Id == order.Id));
            await change.SaveChangesAsync();
        }

        await using var read = fixture.CreateContext();
        Assert.False(await read.OrderItems.AnyAsync(i => i.OrderId == order.Id));
        Assert.True(await read.ProductVariants.AnyAsync(v => v.Id == variant.Id));
    }

    [Fact]
    public async Task Cart_must_belong_to_exactly_one_of_user_or_visitor()
    {
        await using var neither = fixture.CreateContext();
        neither.Carts.Add(new Cart { UpdatedAt = DateTimeOffset.UtcNow });
        await AssertRefusedAsync(neither, 547, "CK_Carts_Exactly_One_Owner");

        await using var both = fixture.CreateContext();
        var user = TestData.User();
        both.Users.Add(user);
        both.Carts.Add(new Cart
        {
            UserId = user.Id,
            VisitorToken = Guid.NewGuid().ToString("N"),
            UpdatedAt = DateTimeOffset.UtcNow,
        });
        await AssertRefusedAsync(both, 547, "CK_Carts_Exactly_One_Owner");
    }

    [Fact]
    public async Task Cart_has_one_row_per_variant()
    {
        await using var context = fixture.CreateContext();
        var variant = TestData.Variant(TestData.Product(TestData.Category()));
        var cart = new Cart { VisitorToken = Guid.NewGuid().ToString("N"), UpdatedAt = DateTimeOffset.UtcNow };
        cart.Items.Add(new CartItem { Variant = variant, Quantity = 1 });
        cart.Items.Add(new CartItem { Variant = variant, Quantity = 2 });
        context.Carts.Add(cart);

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        var sql = Inner(ex);
        Assert.True(sql.Number is 2601 or 2627, $"Unexpected SQL error {sql.Number}");
        Assert.Contains("IX_CartItems_CartId_VariantId", sql.Message);
    }

    [Fact]
    public async Task A_user_has_at_most_one_cart()
    {
        await using var context = fixture.CreateContext();
        var user = TestData.User();
        context.Users.Add(user);
        context.Carts.Add(new Cart { UserId = user.Id, UpdatedAt = DateTimeOffset.UtcNow });
        context.Carts.Add(new Cart { UserId = user.Id, UpdatedAt = DateTimeOffset.UtcNow });

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        var sql = Inner(ex);
        Assert.True(sql.Number is 2601 or 2627, $"Unexpected SQL error {sql.Number}");
        Assert.Contains("IX_Carts_UserId", sql.Message);
    }
}
