using Microsoft.AspNetCore.Identity;

namespace InchesWigStore.Web.Services;

public static class IdentityDefaults
{
    public static void Configure(IdentityOptions options)
    {
        options.Password.RequiredLength = 12;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.User.RequireUniqueEmail = true;
    }
}
