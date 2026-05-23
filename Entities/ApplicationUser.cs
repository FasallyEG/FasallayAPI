using Fasally.Entities.Enums;
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
    public string FullName => $"{FirstName} {LastName}";

    public bool IsDisabled { get; set; } = false;

    public string? ProfileImageUrl { get; set; }

    // Onboarding
    public ProfileType? PendingProfileType { get; set; }
    public bool IsProfileCompleted { get; set; } = false;

    // Navigation
    public Tailor? Tailor { get; set; }
    public SellerProfile? SellerProfile { get; set; }
    public List<RefreshToken> RefreshTokens { get; set; } = [];
    public List<ExternalLogin> ExternalLogins { get; set; } = [];
}
