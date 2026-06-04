namespace Fasally.Contracts.Sellers;

public record PublicSellerProfileResponse(
    string Id,
    string StoreName,
    string? Description,
    string? ShopImageUrl,
    bool IsVerified,
    double AverageRating,
    int TotalReviews
);
