using System.Text.RegularExpressions;
using InchesWigStore.Tests.Infrastructure;
using InchesWigStore.Web.Data;
using InchesWigStore.Web.Models;
using Microsoft.EntityFrameworkCore;
using static InchesWigStore.Tests.Infrastructure.SqlAssert;

namespace InchesWigStore.Tests.Data;

public class PaymentStockSchemaTests(SqlServerFixture fixture) : IClassFixture<SqlServerFixture>
{
    private static Order NewOrder() =>
        TestData.Order(TestData.User(), TestData.Variant(TestData.Product(TestData.Category())));

    private static Payment NewPayment(Order order, string? reference = null, string? providerReference = null) =>
        new()
        {
            Order = order,
            Method = PaymentMethod.Eft,
            Status = PaymentStatus.Pending,
            AmountCents = order.TotalCents,
            Reference = reference ?? $"PAY-{Guid.NewGuid():N}",
            ProviderReference = providerReference,
            CreatedAt = DateTimeOffset.UtcNow,
        };

    private static StockMovement NewMovement(ProductVariant variant, Order? order = null) =>
        new()
        {
            Variant = variant,
            OldQuantity = 5,
            NewQuantity = 4,
            Reason = StockMovementReason.Sale,
            ChangedBy = "system",
            Order = order,
            ChangedAt = DateTimeOffset.UtcNow,
        };

    private static async Task<Payment> SavePaymentAsync(AppDbContext context, string? reference = null, string? providerReference = null)
    {
        var payment = NewPayment(NewOrder(), reference, providerReference);
        context.Payments.Add(payment);
        await context.SaveChangesAsync();
        return payment;
    }

    [Fact]
    public async Task Same_provider_notification_id_twice_is_refused()
    {
        await using var context = fixture.CreateContext();
        var payment = await SavePaymentAsync(context);
        var notificationId = Guid.NewGuid().ToString("N");
        var provider = $"provider-{Guid.NewGuid():N}";

        context.PaymentNotifications.Add(new PaymentNotification
        {
            Provider = provider,
            NotificationId = notificationId,
            PaymentId = payment.Id,
            ReceivedAt = DateTimeOffset.UtcNow,
            Outcome = "Applied",
        });
        await context.SaveChangesAsync();

        // Same NotificationId under a different Provider is a different notification.
        await using (var other = fixture.CreateContext())
        {
            other.PaymentNotifications.Add(new PaymentNotification
            {
                Provider = $"other-{Guid.NewGuid():N}",
                NotificationId = notificationId,
                PaymentId = payment.Id,
                ReceivedAt = DateTimeOffset.UtcNow,
                Outcome = "Applied",
            });
            await other.SaveChangesAsync();
        }

        await using var duplicate = fixture.CreateContext();
        duplicate.PaymentNotifications.Add(new PaymentNotification
        {
            Provider = provider,
            NotificationId = notificationId,
            PaymentId = payment.Id,
            ReceivedAt = DateTimeOffset.UtcNow,
            Outcome = "Duplicate",
        });
        await AssertUniqueViolationAsync(duplicate, "IX_PaymentNotifications_Provider_NotificationId");
    }

    [Fact]
    public async Task Payment_reference_is_unique()
    {
        await using var context = fixture.CreateContext();
        var first = await SavePaymentAsync(context);

        await using var other = fixture.CreateContext();
        other.Payments.Add(NewPayment(NewOrder(), reference: first.Reference));
        await AssertUniqueViolationAsync(other, "IX_Payments_Reference");
    }

    [Fact]
    public async Task Two_payments_without_provider_reference_are_allowed_but_duplicate_provider_reference_is_refused()
    {
        await using var context = fixture.CreateContext();
        await SavePaymentAsync(context);
        await SavePaymentAsync(context);

        var providerReference = $"prov-{Guid.NewGuid():N}";
        await SavePaymentAsync(context, providerReference: providerReference);

        await using var other = fixture.CreateContext();
        other.Payments.Add(NewPayment(NewOrder(), providerReference: providerReference));
        await AssertUniqueViolationAsync(other, "IX_Payments_ProviderReference");
    }

    [Fact]
    public async Task Eft_payment_can_record_amount_received_different_from_amount()
    {
        await using var context = fixture.CreateContext();
        var payment = NewPayment(NewOrder());
        payment.AmountCents = 100_00;
        payment.AmountReceivedCents = 90_00;
        payment.VerifiedBy = "admin@example.com";
        payment.VerifiedAt = DateTimeOffset.UtcNow;
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        await using var read = fixture.CreateContext();
        var reloaded = await read.Payments.SingleAsync(p => p.Id == payment.Id);
        Assert.Equal(100_00, reloaded.AmountCents);
        Assert.Equal(90_00, reloaded.AmountReceivedCents);
        Assert.Equal(PaymentMethod.Eft, reloaded.Method);
        Assert.Equal("admin@example.com", reloaded.VerifiedBy);
    }

    [Fact]
    public async Task Payment_amounts_cannot_be_negative()
    {
        await using var amount = fixture.CreateContext();
        var negativeAmount = NewPayment(NewOrder());
        negativeAmount.AmountCents = -1;
        amount.Payments.Add(negativeAmount);
        await AssertRefusedAsync(amount, 547, "CK_Payments_Amount_NonNegative");

        await using var received = fixture.CreateContext();
        var negativeReceived = NewPayment(NewOrder());
        negativeReceived.AmountReceivedCents = -1;
        received.Payments.Add(negativeReceived);
        await AssertRefusedAsync(received, 547, "CK_Payments_AmountReceived_NonNegative");
    }

    [Fact]
    public async Task Order_with_a_payment_cannot_be_deleted()
    {
        await using var context = fixture.CreateContext();
        var payment = await SavePaymentAsync(context);

        await using var other = fixture.CreateContext();
        var items = await other.OrderItems.Where(i => i.OrderId == payment.OrderId).ToListAsync();
        other.OrderItems.RemoveRange(items);
        other.Orders.Remove(await other.Orders.SingleAsync(o => o.Id == payment.OrderId));
        await AssertRefusedAsync(other, 547, "FK_Payments_Orders_OrderId");
    }

    [Fact]
    public async Task Stock_movement_records_old_new_reason_and_actor()
    {
        await using var context = fixture.CreateContext();
        var order = NewOrder();
        var movement = NewMovement(order.Items[0].Variant, order);
        movement.OldQuantity = 5;
        movement.NewQuantity = 8;
        movement.Reason = StockMovementReason.AdminAdjustment;
        movement.ChangedBy = "admin@example.com";
        context.StockMovements.Add(movement);
        await context.SaveChangesAsync();

        await using var read = fixture.CreateContext();
        var reloaded = await read.StockMovements.SingleAsync(m => m.Id == movement.Id);
        Assert.Equal(movement.VariantId, reloaded.VariantId);
        Assert.Equal(5, reloaded.OldQuantity);
        Assert.Equal(8, reloaded.NewQuantity);
        Assert.Equal(StockMovementReason.AdminAdjustment, reloaded.Reason);
        Assert.Equal("admin@example.com", reloaded.ChangedBy);
        Assert.Equal(order.Id, reloaded.OrderId);

        var raw = await read.Database
            .SqlQuery<string>($"SELECT [Reason] AS [Value] FROM [StockMovements] WHERE [Id] = {movement.Id}")
            .SingleAsync();
        Assert.Equal("AdminAdjustment", raw);
    }

    [Fact]
    public async Task Stock_movement_with_negative_quantity_is_refused()
    {
        var variant = TestData.Variant(TestData.Product(TestData.Category()));

        await using var oldQuantity = fixture.CreateContext();
        var negativeOld = NewMovement(variant);
        negativeOld.OldQuantity = -1;
        oldQuantity.StockMovements.Add(negativeOld);
        await AssertRefusedAsync(oldQuantity, 547, "CK_StockMovements_OldQuantity_NonNegative");

        await using var newQuantity = fixture.CreateContext();
        var negativeNew = NewMovement(TestData.Variant(TestData.Product(TestData.Category())));
        negativeNew.NewQuantity = -1;
        newQuantity.StockMovements.Add(negativeNew);
        await AssertRefusedAsync(newQuantity, 547, "CK_StockMovements_NewQuantity_NonNegative");
    }

    [Fact]
    public async Task Variant_with_stock_movements_cannot_be_deleted()
    {
        await using var context = fixture.CreateContext();
        var movement = NewMovement(TestData.Variant(TestData.Product(TestData.Category())));
        context.StockMovements.Add(movement);
        await context.SaveChangesAsync();

        await using var other = fixture.CreateContext();
        other.ProductVariants.Remove(await other.ProductVariants.SingleAsync(v => v.Id == movement.VariantId));
        await AssertRefusedAsync(other, 547, "FK_StockMovements_ProductVariants_VariantId");
    }

    [Fact]
    public async Task Order_with_stock_movements_cannot_be_deleted()
    {
        await using var context = fixture.CreateContext();
        var order = NewOrder();
        var movement = NewMovement(order.Items[0].Variant, order);
        context.StockMovements.Add(movement);
        await context.SaveChangesAsync();

        await using var other = fixture.CreateContext();
        var items = await other.OrderItems.Where(i => i.OrderId == order.Id).ToListAsync();
        other.OrderItems.RemoveRange(items);
        other.Orders.Remove(await other.Orders.SingleAsync(o => o.Id == order.Id));
        await AssertRefusedAsync(other, 547, "FK_StockMovements_Orders_OrderId");
    }

    [Fact]
    public void Schema_has_no_column_that_could_hold_card_data()
    {
        using var context = fixture.CreateContext();

        // "Card", "Cvv" and "Cvc" are matched as case-insensitive substrings. "Pan" is matched
        // only as a whole PascalCase word (e.g. "CardPan", "pan_number") so that legitimate names
        // such as "Company" or "Expansion" are not false positives.
        static bool IsSuspect(string name)
        {
            if (name.Contains("Card", StringComparison.OrdinalIgnoreCase)
                || name.Contains("Cvv", StringComparison.OrdinalIgnoreCase)
                || name.Contains("Cvc", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var words = Regex.Split(name, @"(?<=[a-z0-9])(?=[A-Z])|[^A-Za-z0-9]+");
            return words.Any(w => w.Equals("Pan", StringComparison.OrdinalIgnoreCase));
        }

        // Guard the word splitter itself.
        Assert.True(IsSuspect("CardNumber"));
        Assert.True(IsSuspect("Pan"));
        Assert.True(IsSuspect("CardPan"));
        Assert.True(IsSuspect("pan_number"));
        Assert.True(IsSuspect("SecurityCvv"));
        Assert.False(IsSuspect("Company"));
        Assert.False(IsSuspect("Expansion"));

        var offenders = context.Model.GetEntityTypes()
            .SelectMany(e => e.GetProperties().Select(p => $"{e.DisplayName()}.{p.Name}"))
            .Where(full => IsSuspect(full[(full.LastIndexOf('.') + 1)..]))
            .ToList();
        Assert.Empty(offenders);
    }
}
