using Fasally.Entities.Enums;

namespace Fasally.Contracts.Users;

public record TailorStatusResponse(
    ProfileStatus Status,
    bool          IsVerified,
    int           ExperienceYears
);
