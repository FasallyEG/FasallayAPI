using Fasally.Entities.Enums;

namespace Fasally.Contracts.Sellers;

public record SellerProfileResponse(
    string Id,
    string StoreName,
    string? Description,
    string? BusinessPhone,
    string? BusinessEmail,
    string? ShopImageUrl,
    ProfileStatus Status,
    bool IsVerified,
    double AverageRating,
    int TotalReviews
);

public record PublicSellerProfileResponse(
    string Id,
    string StoreName,
    string? Description,
    string? ShopImageUrl,
    bool IsVerified,
    double AverageRating,
    int TotalReviews
);
