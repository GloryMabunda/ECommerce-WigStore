using Microsoft.AspNetCore.Identity;

namespace InchesWigStore.Web.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
