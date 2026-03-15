using Microsoft.AspNetCore.Identity;

namespace Fasally.Entities;

public sealed class ApplicationRole : IdentityRole
{
    public ApplicationRole() => Id = Guid.NewGuid().ToString();

    public bool IsDefault { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
}