using Fasally.Abstractions;

namespace Fasally.Errors;

public static class TailorErrors
{
    public static readonly Error TailorNotFound =
        new("Tailor.NotFound", "Tailor profile not found", StatusCodes.Status404NotFound);

    public static readonly Error TailorAlreadyExists =
        new("Tailor.AlreadyExists", "A tailor profile already exists for this user", StatusCodes.Status409Conflict);

    public static readonly Error NotApprovedYet =
        new("Tailor.NotApprovedYet", "Your upgrade request has not been approved yet", StatusCodes.Status403Forbidden);

    public static readonly Error ProfileAlreadyApproved =
        new("Tailor.ProfileAlreadyApproved", "Tailor upgrade request is already approved", StatusCodes.Status409Conflict);

    public static readonly Error ProfileNotPending =
        new("Tailor.ProfileNotPending", "Tailor upgrade request is not in a pending state", StatusCodes.Status400BadRequest);

    public static readonly Error InvalidCategories =
        new("Tailor.InvalidCategories", "One or more selected categories are invalid", StatusCodes.Status400BadRequest);

    public static readonly Error PortfolioItemNotFound =
        new("Tailor.PortfolioItemNotFound", "Portfolio item not found", StatusCodes.Status404NotFound);
}
