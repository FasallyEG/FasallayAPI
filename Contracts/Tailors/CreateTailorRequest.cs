namespace Fasally.Contracts.Tailors;

public record CreateTailorRequest(
    int ExperienceYears,
    List<int>? CategoryIds,
    string? Bio,
    string? NationalIdImageUrl,
    string? ShopImageUrl
);
