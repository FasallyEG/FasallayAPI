using Fasally.Abstractions.Consts;
using Fasally.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Fasally.Persistence.Seed;

public static class RoleClaimSeeder
{
    public static async Task SeedRoleClaimsAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider
            .GetRequiredService<RoleManager<ApplicationRole>>();

        // Admin => all permissions
        await SeedClaimsForRoleAsync(
            roleManager,
            DefaultRoles.Admin,
            Permissions.GetAllPermissions());

        // Tailor permissions
        await SeedClaimsForRoleAsync(
            roleManager,
            DefaultRoles.Tailor,
            Permissions.TailorPermissions);

        // Seller permissions
        await SeedClaimsForRoleAsync(
            roleManager,
            DefaultRoles.Seller,
            Permissions.SellerPermissions);

        // Member permissions
        await SeedClaimsForRoleAsync(
            roleManager,
            DefaultRoles.Member,
            Permissions.MemberPermissions);
    }

    private static async Task SeedClaimsForRoleAsync(
        RoleManager<ApplicationRole> roleManager,
        string roleName,
        IEnumerable<string> permissions)
    {
        var role = await roleManager.FindByNameAsync(roleName);

        if (role is null)
            return;

        var existingClaims = await roleManager.GetClaimsAsync(role);

        foreach (var permission in permissions)
        {
            var hasPermission = existingClaims.Any(c =>
                c.Type == Permissions.Type &&
                c.Value == permission);

            if (hasPermission)
                continue;

            await roleManager.AddClaimAsync(
                role,
                new Claim(Permissions.Type, permission));
        }
    }
}
