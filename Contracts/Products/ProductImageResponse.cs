namespace Fasally.Contracts.Products;

public record ProductImageResponse(
    Guid Id,
    string ImageUrl,
    string? AltText,
    int SortOrder
);
