using InchesWigStore.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace InchesWigStore.Web.Services;

public partial class AdminSeeder(
    UserManager<ApplicationUser> users,
    RoleManager<IdentityRole<Guid>> roles,
    ILogger<AdminSeeder> logger)
{
    public async Task SeedAsync(string? email, string? password)
    {
        foreach (var role in new[] { Roles.Administrator, Roles.Customer })
        {
            if (!await roles.RoleExistsAsync(role))
            {
                Check(await roles.CreateAsync(new IdentityRole<Guid>(role)), $"create role {role}");
            }
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
        {
            return;
        }

        var existing = await users.FindByEmailAsync(email);
        if (existing is not null)
        {
            if (!await users.IsInRoleAsync(existing, Roles.Administrator))
            {
                // Promote only when the caller proves control of the account; never log the email or password.
                if (!await users.CheckPasswordAsync(existing, password))
                {
                    LogNotPromoted();
                    return;
                }

                Check(await users.AddToRoleAsync(existing, Roles.Administrator), "assign the Administrator role");
            }

            return;
        }

        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = "Administrator",
            CreatedAt = DateTimeOffset.UtcNow,
        };
        Check(await users.CreateAsync(admin, password), "create the administrator");
        Check(await users.AddToRoleAsync(admin, Roles.Administrator), "assign the Administrator role");
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "An account with the configured administrator email exists but does not use the configured password; it was not promoted.")]
    private partial void LogNotPromoted();

    private static void Check(IdentityResult result, string action)
    {
        if (result.Succeeded)
        {
            return;
        }

        var details = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
        throw new InvalidOperationException($"Failed to {action}. {details}");
    }
}
