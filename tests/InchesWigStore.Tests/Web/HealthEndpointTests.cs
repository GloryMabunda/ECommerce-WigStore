using InchesWigStore.Tests.Infrastructure;
using InchesWigStore.Web.Models;
using InchesWigStore.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace InchesWigStore.Tests.Web;

public class HealthEndpointTests(SqlServerFixture fixture) : IClassFixture<SqlServerFixture>
{
    private const string _unreachable =
        "Server=tcp:does-not-exist.invalid,1433;Database=x;User Id=u;Password=p;Connect Timeout=1;Encrypt=false";

    private static TestFactory CreateFactory(
        string connectionString, string? adminEmail = null, string? adminPassword = null) =>
        new(connectionString, adminEmail, adminPassword);

    private sealed class TestFactory(string connectionString, string? adminEmail, string? adminPassword)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);

            // Blank unless the test sets them, so ambient Admin__* variables on a developer machine cannot leak in.
            builder.UseSetting("Admin:Email", adminEmail ?? string.Empty);
            builder.UseSetting("Admin:Password", adminPassword ?? string.Empty);
        }
    }

    [Fact]
    public async Task Health_returns_200_when_database_is_reachable()
    {
        using var factory = CreateFactory(fixture.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Health_returns_503_when_database_is_unreachable()
    {
        using var factory = CreateFactory(_unreachable);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(System.Net.HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("Unhealthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Admin_is_seeded_from_configuration_at_startup()
    {
        const string email = "seeded.admin@example.com";
        using var factory = CreateFactory(fixture.ConnectionString, email, "Str0ng!Passw0rd#1");

        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync(email);

        Assert.NotNull(user);
        Assert.True(await users.IsInRoleAsync(user, Roles.Administrator));
    }

    [Fact]
    public async Task Home_page_still_renders()
    {
        using var factory = CreateFactory(fixture.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public void Identity_options_match_the_security_requirements()
    {
        using var factory = CreateFactory(fixture.ConnectionString);
        var options = factory.Services.GetRequiredService<IOptions<IdentityOptions>>().Value;

        Assert.Equal(12, options.Password.RequiredLength);
        Assert.Equal(5, options.Lockout.MaxFailedAccessAttempts);
        Assert.Equal(TimeSpan.FromMinutes(15), options.Lockout.DefaultLockoutTimeSpan);
        Assert.True(options.User.RequireUniqueEmail);
    }
}
