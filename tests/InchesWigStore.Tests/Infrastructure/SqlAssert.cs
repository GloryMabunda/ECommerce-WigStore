using InchesWigStore.Web.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace InchesWigStore.Tests.Infrastructure;

/// <summary>Assertions that the database itself refused a save, naming the exact constraint.</summary>
public static class SqlAssert
{
    public static SqlException Inner(DbUpdateException ex) =>
        Assert.IsType<SqlException>(ex.InnerException);

    public static async Task<SqlException> AssertRefusedAsync(AppDbContext context, int number, string constraint)
    {
        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        var sql = Inner(ex);
        Assert.Equal(number, sql.Number);
        Assert.Contains(constraint, sql.Message);
        return sql;
    }

    /// <summary>A unique index or constraint violation (SQL Server error 2601 or 2627) naming <paramref name="index"/>.</summary>
    public static async Task<SqlException> AssertUniqueViolationAsync(AppDbContext context, string index)
    {
        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        var sql = Inner(ex);
        Assert.True(sql.Number is 2601 or 2627, $"Unexpected SQL error {sql.Number}");
        Assert.Contains(index, sql.Message);
        return sql;
    }
}
