namespace Fasally.Contracts.Tailors;

public record CreatePortfolioItemRequest(
    string Title,
    string Description,
    List<string> ImageUrls
);
