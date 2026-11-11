using InchesWigStore.Tests.Infrastructure;
using InchesWigStore.Web.Data;
using InchesWigStore.Web.Models;
using InchesWigStore.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InchesWigStore.Tests.Services;

public class AdminSeederTests(SqlServerFixture fixture) : IClassFixture<SqlServerFixture>
{
    private const string _strongPassword = "Str0ng!Passw0rd#1";

    private ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(o => o.UseSqlServer(fixture.ConnectionString));
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(IdentityDefaults.Configure)
            .AddEntityFrameworkStores<AppDbContext>();
        services.AddScoped<AdminSeeder>();
        return services.BuildServiceProvider();
    }

    private static string NewEmail() => $"admin-{Guid.NewGuid():N}@example.com";

    [Fact]
    public async Task Creates_roles_and_one_administrator()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var email = NewEmail();

        await scope.ServiceProvider.GetRequiredService<AdminSeeder>().SeedAsync(email, _strongPassword);

        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        Assert.True(await roles.RoleExistsAsync(Roles.Administrator));
        Assert.True(await roles.RoleExistsAsync(Roles.Customer));
        var user = await users.FindByEmailAsync(email);
        Assert.NotNull(user);
        Assert.True(user.EmailConfirmed);
        Assert.True(await users.IsInRoleAsync(user, Roles.Administrator));
    }

    [Fact]
    public async Task Running_twice_creates_one_administrator()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var email = NewEmail();
        var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();

        await seeder.SeedAsync(email, _strongPassword);
        await seeder.SeedAsync(email, _strongPassword);

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(1, await context.Users.CountAsync(u => u.Email == email));
        Assert.Equal(1, await context.Roles.CountAsync(r => r.Name == Roles.Administrator));
    }

    [Fact]
    public async Task Missing_password_creates_roles_but_no_user()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var email = NewEmail();

        await scope.ServiceProvider.GetRequiredService<AdminSeeder>().SeedAsync(email, null);

        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        Assert.True(await roles.RoleExistsAsync(Roles.Administrator));
        Assert.True(await roles.RoleExistsAsync(Roles.Customer));
        Assert.Null(await users.FindByEmailAsync(email));
    }

    [Fact]
    public async Task Seeded_user_is_only_in_the_Administrator_role()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var email = NewEmail();

        await scope.ServiceProvider.GetRequiredService<AdminSeeder>().SeedAsync(email, _strongPassword);

        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync(email);
        Assert.NotNull(user);
        var userRoles = await users.GetRolesAsync(user);
        Assert.Equal([Roles.Administrator], userRoles);
        Assert.False(await users.IsInRoleAsync(user, Roles.Customer));
    }

    [Fact]
    public async Task Weak_password_fails_with_an_error_and_creates_no_user()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var email = NewEmail();
        const string weak = "short";

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => scope.ServiceProvider.GetRequiredService<AdminSeeder>().SeedAsync(email, weak));

        Assert.DoesNotContain(weak, ex.Message);
        Assert.Contains("PasswordTooShort", ex.Message);
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.Null(await users.FindByEmailAsync(email));
    }

    [Fact]
    public async Task Existing_user_without_the_Administrator_role_is_repaired()
    {
        using var provider = BuildProvider();
        var email = NewEmail();
        using (var first = provider.CreateScope())
        {
            await first.ServiceProvider.GetRequiredService<AdminSeeder>().SeedAsync(null, null);
            var users = first.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var created = await users.CreateAsync(
                new ApplicationUser { UserName = email, Email = email, DisplayName = "Partial", CreatedAt = DateTimeOffset.UtcNow },
                _strongPassword);
            Assert.True(created.Succeeded);
        }

        using var second = provider.CreateScope();
        await second.ServiceProvider.GetRequiredService<AdminSeeder>().SeedAsync(email, _strongPassword);

        var secondUsers = second.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await secondUsers.FindByEmailAsync(email);
        Assert.NotNull(user);
        Assert.True(await secondUsers.IsInRoleAsync(user, Roles.Administrator));
        var context = second.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(1, await context.Users.CountAsync(u => u.Email == email));
    }

    [Fact]
    public async Task Existing_user_with_a_different_password_is_not_promoted()
    {
        using var provider = BuildProvider();
        var email = NewEmail();
        using (var first = provider.CreateScope())
        {
            await first.ServiceProvider.GetRequiredService<AdminSeeder>().SeedAsync(null, null);
            var users = first.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var created = await users.CreateAsync(
                new ApplicationUser { UserName = email, Email = email, DisplayName = "Someone", CreatedAt = DateTimeOffset.UtcNow },
                "An0ther!Passw0rd#2");
            Assert.True(created.Succeeded);
        }

        using var second = provider.CreateScope();
        await second.ServiceProvider.GetRequiredService<AdminSeeder>().SeedAsync(email, _strongPassword);

        var secondUsers = second.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await secondUsers.FindByEmailAsync(email);
        Assert.NotNull(user);
        Assert.False(await secondUsers.IsInRoleAsync(user, Roles.Administrator));
        var context = second.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(1, await context.Users.CountAsync(u => u.Email == email));
    }
}
