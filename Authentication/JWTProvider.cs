using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Fasally.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Fasally.Abstractions.Constants;

namespace Fasally.Authentication;

public class JWTProvider(IOptions<JWTOptions> jwtOptions, UserManager<ApplicationUser> userManager) : IJWTProvider
{
    private readonly JWTOptions _jwtOptions = jwtOptions.Value;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<(string token, int expiresIn)> GenerateTokenAsync(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName)
        };

        var roles = await _userManager.GetRolesAsync(user);

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtOptions.Key));

        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return (
            token: new JwtSecurityTokenHandler().WriteToken(token),
            expiresIn: _jwtOptions.ExpiryMinutes * 60
        );
    }

    public string? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtOptions.Key);

        try
        {
            tokenHandler.ValidateToken(token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _jwtOptions.Issuer,
                    ValidAudience = _jwtOptions.Audience,
                    ClockSkew = TimeSpan.Zero
                },
                out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            return jwtToken.Claims
                .First(x => x.Type == JwtRegisteredClaimNames.Sub)
                .Value;
        }
        catch
        {
            return null;
        }
    }
}
