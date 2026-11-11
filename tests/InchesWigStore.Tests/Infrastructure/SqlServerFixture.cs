using InchesWigStore.Web.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace InchesWigStore.Tests.Infrastructure;

/// <summary>
/// Creates a uniquely named SQL Server database, applies all migrations, and drops it afterwards.
/// Uses TEST_SQL_CONNECTION when set, otherwise SQL Server LocalDB.
/// </summary>
public sealed class SqlServerFixture : IAsyncLifetime
{
    private string _serverConnection = string.Empty;
    private string _databaseName = string.Empty;
    private string _connectionString = string.Empty;

    public async Task InitializeAsync()
    {
        const string defaultServerConnection = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true";
        var configured = Environment.GetEnvironmentVariable("TEST_SQL_CONNECTION");
        _serverConnection = string.IsNullOrWhiteSpace(configured) ? defaultServerConnection : configured;
        _databaseName = $"InchesWigStore_Test_{Guid.NewGuid():N}";
        _connectionString = new SqlConnectionStringBuilder(_serverConnection)
        {
            InitialCatalog = _databaseName,
            TrustServerCertificate = true,
        }.ConnectionString;

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(_connectionString)
            .Options;
        return new AppDbContext(options);
    }

    public async Task DisposeAsync()
    {
        if (_databaseName.Length == 0)
        {
            return;
        }

        SqlConnection.ClearAllPools();
        await using var connection = new SqlConnection(new SqlConnectionStringBuilder(_serverConnection)
        {
            TrustServerCertificate = true,
        }.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText =
            $"ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{_databaseName}];";
        await command.ExecuteNonQueryAsync();
    }

}
