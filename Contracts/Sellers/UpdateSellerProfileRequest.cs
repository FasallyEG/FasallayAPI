namespace Fasally.Contracts.Sellers;

public record UpdateSellerProfileRequest(
    string StoreName,
    string? Description,
    string? BusinessPhone,
    string? BusinessEmail,
    string? ShopImageUrl
);
