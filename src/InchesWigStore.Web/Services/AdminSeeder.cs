using InchesWigStore.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace InchesWigStore.Web.Services;

public class AdminSeeder(UserManager<ApplicationUser> users, RoleManager<IdentityRole<Guid>> roles)
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

        if (await users.FindByEmailAsync(email) is not null)
        {
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
