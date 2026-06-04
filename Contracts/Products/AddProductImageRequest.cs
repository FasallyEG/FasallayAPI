namespace Fasally.Contracts.Products;

public record AddProductImageRequest(
    string ImageUrl,
    string? AltText,
    int SortOrder = 0
);
