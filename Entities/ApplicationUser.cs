using Microsoft.AspNetCore.Identity;

namespace Fasally.Entities;

public sealed class ApplicationUser : IdentityUser
{
    public ApplicationUser()
    {
        Id = Guid.NewGuid().ToString();
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsDisabled { get; set; } = false;

    public string? ProfileImageUrl { get; set; }

    public List<RefreshToken> RefreshTokens { get; set; } = [];
    public List<ExternalLogin> ExternalLogins { get; set; } = [];
}
