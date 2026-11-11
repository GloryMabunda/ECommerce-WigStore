using InchesWigStore.Tests.Infrastructure;
using InchesWigStore.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace InchesWigStore.Tests.Data;

public class IdentitySchemaTests(SqlServerFixture fixture) : IClassFixture<SqlServerFixture>
{
    private static Address NewAddress(Guid userId) => new()
    {
        UserId = userId,
        RecipientName = "Test Person",
        Phone = "0820000000",
        Line1 = "1 Test Street",
        City = "Johannesburg",
        Province = "Gauteng",
        PostalCode = "2000",
        IsDefault = true,
    };

    [Fact]
    public async Task Address_requires_an_existing_user()
    {
        await using var context = fixture.CreateContext();
        context.Addresses.Add(NewAddress(Guid.NewGuid()));

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());

        var sql = SqlAssert.Inner(ex);
        Assert.Equal(547, sql.Number);
        Assert.Contains("FK_Addresses_AspNetUsers_UserId", sql.Message);
    }

    [Fact]
    public async Task Deleting_a_user_with_addresses_is_refused()
    {
        await using var context = fixture.CreateContext();
        var user = TestData.User();
        context.Users.Add(user);
        context.Addresses.Add(NewAddress(user.Id));
        await context.SaveChangesAsync();

        await using var other = fixture.CreateContext();
        other.Users.Remove(await other.Users.SingleAsync(u => u.Id == user.Id));
        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => other.SaveChangesAsync());

        var sql = SqlAssert.Inner(ex);
        Assert.Equal(547, sql.Number);
        Assert.Contains("FK_Addresses_AspNetUsers_UserId", sql.Message);
    }
}
