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
    public const string ViewMyMeasurements = "measurements:read";
    public const string UpdateMyMeasurements = "measurements:update";

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
    // Proposals
    // =============================
    public const string CreateProposal = "proposals:create";
    public const string ViewMyProposals = "proposals:read";
    public const string ApproveProposal = "proposals:approve";
    public const string RejectProposal = "proposals:reject";
    public const string ViewTailorProposals = "tailor:proposals:read";
    public const string AcceptProposal = "tailor:proposals:accept";
    public const string RejectTailorProposal = "tailor:proposals:reject";
    public const string FinalizeProposal = "tailor:proposals:finalize";

    // =============================
    // Projects
    // =============================
    public const string ViewMyProjects = "projects:read";
    public const string StartProject = "projects:start";
    public const string CompleteProject = "projects:complete";

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
    public const string ViewMySellerProducts = "seller:products:read";
    public const string CreateProduct        = "products:create";
    public const string UpdateProduct        = "products:update";
    public const string DeleteProduct        = "products:delete";
    public const string AddProductImage      = "products:images:add";
    public const string DeleteProductImage   = "products:images:delete";
    public const string AddProductVariant    = "products:variants:add";
    public const string UpdateProductVariant = "products:variants:update";
    public const string DeleteProductVariant = "products:variants:delete";
    public const string UpdateProductStock   = "products:stock:update";
    public const string ViewProductInventory = "products:inventory:read";
    public const string ViewSellerDashboard  = "seller:dashboard:read";

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
        ViewMyMeasurements,
        UpdateMyMeasurements,
        ViewTailors,
        ViewTailorDetails,
        CreateProposal,
        ViewMyProposals,
        ApproveProposal,
        RejectProposal,
        ViewMyProjects,
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
        ViewMyMeasurements,
        UpdateMyMeasurements,
        ViewTailors,
        ViewTailorDetails,
        ViewMyProposals,
        ViewTailorProposals,
        AcceptProposal,
        RejectTailorProposal,
        FinalizeProposal,
        ViewMyProjects,
        StartProject,
        CompleteProject,
        CreateTailorProfile,
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
        ViewMyMeasurements,
        UpdateMyMeasurements,
        ViewTailors,
        ViewTailorDetails,
        CreateProposal,
        ViewMyProposals,
        ApproveProposal,
        RejectProposal,
        ViewMyProjects,
        ViewSellerProfile,
        CreateSellerProfile,
        ViewMySellerProfile,
        UpdateSellerProfile,
        ViewMySellerProducts,
        CreateProduct,
        UpdateProduct,
        DeleteProduct,
        AddProductImage,
        DeleteProductImage,
        AddProductVariant,
        UpdateProductVariant,
        DeleteProductVariant,
        UpdateProductStock,
        ViewProductInventory,
        ViewSellerDashboard
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
