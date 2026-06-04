using Fasally.Entities.Enums;

namespace Fasally.Contracts.Users;

public record RequestUpgradeRequest(
    ProfileType ProfileType
);
