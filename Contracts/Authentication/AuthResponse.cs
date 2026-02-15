namespace Fasally.Contracts.Authentication;

public record AuthResponse(
    string Id,
    string? Email,
    string FirstName,
    string LastName,
    string Token,
    int Expiredin,
    string RefreshToken,
    DateTime RefreshTokenExpiration
);