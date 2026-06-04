namespace Fasally.Contracts.Tailors;

public record TailorListItemResponse(
    string              Id,
    string              FullName,
    string?             ProfileImageUrl,
    IEnumerable<string> Categories,
    int                 ExperienceYears,
    double              AverageRating,
    int                 TotalReviews
);
