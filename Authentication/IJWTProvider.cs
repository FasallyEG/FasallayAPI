using Fasally.Entities;

namespace Fasally.Authentication;

public interface IJWTProvider
{
    (string token, int expiresIn) GenerateToken(ApplicationUser user);
    string? ValidateToken(string token);
}
