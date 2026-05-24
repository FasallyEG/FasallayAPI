using Fasally.Abstractions;

namespace Fasally.Errors;

public static class SellerErrors
{
    public static readonly Error SellerNotFound =
        new("Seller.NotFound", "Seller profile not found", StatusCodes.Status404NotFound);

    public static readonly Error SellerAlreadyExists =
        new("Seller.AlreadyExists", "A seller profile already exists for this user", StatusCodes.Status409Conflict);

    public static readonly Error NotApprovedYet =
        new("Seller.NotApprovedYet", "Your seller upgrade request has not been approved yet", StatusCodes.Status403Forbidden);
}
