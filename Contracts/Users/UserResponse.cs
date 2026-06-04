using Fasally.Entities.Enums;

namespace Fasally.Contracts.Users;

public record UserResponse(
    string               Id,
    string               FirstName,
    string               LastName,
    string               Email,
    bool                 IsDisabled,
    IEnumerable<string>  Roles,
    TailorStatusResponse? TailorProfile
);
