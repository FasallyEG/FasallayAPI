using Microsoft.AspNetCore.Http;

namespace Fasally.Contracts.Authentication;

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    IFormFile? ProfileImage
);