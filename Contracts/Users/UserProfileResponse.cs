using Fasally.Entities.Enums;

namespace Fasally.Contracts.Users;

public class UserProfileResponse
{
    public string  Id       { get; set; } = null!;
    public string  Email    { get; set; } = null!;
    public string  UserName { get; set; } = null!;
    public string  FirstName { get; set; } = null!;
    public string  LastName  { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }

    public bool         NeedsProfileCompletion { get; set; }
    public ProfileType? PendingProfileType     { get; set; }

    public TailorStatusResponse? TailorProfile { get; set; }
}
