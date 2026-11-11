using InchesWigStore.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InchesWigStore.Tests.Data;

public class MigrationTests(SqlServerFixture fixture) : IClassFixture<SqlServerFixture>
{
    [Fact]
    public void Model_has_no_pending_changes()
    {
        using var context = fixture.CreateContext();

        Assert.False(context.Database.HasPendingModelChanges());
    }
}
