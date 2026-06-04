namespace Fasally.Contracts.Tailors;

public record PortfolioItemResponse(
    int                 Id,
    string              Title,
    string              Description,
    IEnumerable<string> ImageUrls
);
