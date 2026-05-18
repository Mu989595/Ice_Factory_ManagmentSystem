using Microsoft.AspNetCore.Identity;

namespace IcePlant.Domain.Identity;

/// <summary>
/// Custom application user extending the default ASP.NET Core Identity user.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
