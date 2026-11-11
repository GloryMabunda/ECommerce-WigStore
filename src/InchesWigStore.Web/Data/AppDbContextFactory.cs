using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InchesWigStore.Web.Data;

/// <summary>Used only by EF Core tooling (migrations); the app registers the context in Program.cs later.</summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=InchesWigStore_Design;Integrated Security=true")
            .Options;
        return new AppDbContext(options);
    }
}
