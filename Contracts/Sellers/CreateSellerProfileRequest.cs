namespace Fasally.Contracts.Sellers;

public record CreateSellerProfileRequest(
    string StoreName,
    string? Description,
    string? BusinessPhone,
    string? BusinessEmail,
    string? ShopImageUrl
);
