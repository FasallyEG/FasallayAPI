using System.Reflection;

namespace Fasally.Abstractions.Consts;

public static class Permissions
{
    public static string Type { get; } = "permissions";

    // =============================
    // Profile / Client
    // =============================
    public const string GetMyProfile    = "profile:read";
    public const string UpdateMyProfile = "profile:update";
    public const string ChangePassword  = "profile:change-password";
    public const string CompleteProfile = "profile:complete";

    // =============================
    // Admin - Users
    // =============================
    public const string GetUsers    = "users:read";
    public const string AddUsers    = "users:add";
    public const string UpdateUsers = "users:update";

    // =============================
    // Roles
    // =============================
    public const string GetRoles    = "roles:read";
    public const string AddRoles    = "roles:add";
    public const string UpdateRoles = "roles:update";

    // =============================
    // Tailor Browsing
    // =============================
    public const string ViewTailors       = "tailors:browse";
    public const string ViewTailorDetails = "tailors:details";

    // =============================
    // Tailor
    // =============================
    public const string RequestTailorUpgrade  = "tailor:request-upgrade";
    public const string CreateTailorProfile   = "tailor:create";
    public const string UpdateTailorProfile   = "tailor:update";
    public const string AddPortfolioItem      = "tailor:portfolio:add";
    public const string ViewMyPortfolio       = "tailor:portfolio:read";

    // =============================
    // Seller
    // =============================
    public const string CreateSellerProfile = "seller:profile:create";
    public const string ViewMySellerProfile = "seller:profile:read";
    public const string ViewSellerProfile   = "seller:profile:details";
    public const string UpdateSellerProfile = "seller:profile:update";

    // =============================
    // Tailor Admin
    // =============================
    public const string ApproveTailor = "tailors:approve";
    public const string RejectTailor  = "tailors:reject";

    // =============================
    // Categories
    // =============================
    public const string GetCategories    = "categories:read";
    public const string AddCategories    = "categories:add";
    public const string UpdateCategories = "categories:update";
    public const string DeleteCategories = "categories:delete";

    // =============================
    // Member Permissions
    // =============================
    public static readonly IReadOnlyCollection<string> MemberPermissions =
    [
        GetMyProfile,
        UpdateMyProfile,
        ChangePassword,
        CompleteProfile,
        ViewTailors,
        ViewTailorDetails,
        RequestTailorUpgrade,
        CreateTailorProfile,
        ViewSellerProfile,
        CreateSellerProfile
    ];

    // =============================
    // Tailor Permissions
    // =============================
    public static readonly IReadOnlyCollection<string> TailorPermissions =
    [
        GetMyProfile,
        UpdateMyProfile,
        ChangePassword,
        ViewTailors,
        ViewTailorDetails,
        UpdateTailorProfile,
        AddPortfolioItem,
        ViewMyPortfolio
    ];

    // =============================
    // Seller Permissions
    // =============================
    public static readonly IReadOnlyCollection<string> SellerPermissions =
    [
        GetMyProfile,
        UpdateMyProfile,
        ChangePassword,
        ViewTailors,
        ViewTailorDetails,
        ViewSellerProfile,
        CreateSellerProfile,
        ViewMySellerProfile,
        UpdateSellerProfile
    ];

    // =============================
    // Helper
    // =============================
    public static IReadOnlyCollection<string> GetAllPermissions() =>
        typeof(Permissions)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral && !f.IsInitOnly)
            .Select(f => f.GetRawConstantValue()?.ToString())
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Cast<string>()
            .ToList();
}
