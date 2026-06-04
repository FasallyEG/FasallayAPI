namespace Fasally.Contracts.Tailors;

public record UpdateTailorRequest(
    int ExperienceYears,
    List<int>? CategoryIds,
    string? Bio
);
