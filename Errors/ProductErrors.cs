using Fasally.Abstractions;

namespace Fasally.Errors;

public static class ProductErrors
{
    public static readonly Error ProductNotFound =
        new("Product.NotFound", "Product not found", StatusCodes.Status404NotFound);

    public static readonly Error ProductImageNotFound =
        new("Product.ImageNotFound", "Product image not found", StatusCodes.Status404NotFound);

    public static readonly Error ProductVariantNotFound =
        new("Product.VariantNotFound", "Product variant not found", StatusCodes.Status404NotFound);

    public static readonly Error NotProductOwner =
        new("Product.NotProductOwner", "Only the owning seller can manage this product", StatusCodes.Status403Forbidden);

    public static readonly Error InvalidPrice =
        new("Product.InvalidPrice", "Product price must be greater than zero", StatusCodes.Status400BadRequest);

    public static readonly Error InvalidStock =
        new("Product.InvalidStock", "Product stock cannot be negative", StatusCodes.Status400BadRequest);
}
