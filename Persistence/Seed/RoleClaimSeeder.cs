using Fasally.Abstractions.Consts;
using Fasally.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Fasally.Persistence.Seed;

public static class RoleClaimSeeder
{
    public static async Task SeedRoleClaimsAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        var adminRole = await roleManager.FindByIdAsync(DefaultRoles.Admin.Id);

        if (adminRole is null)
            return;

        var permissions = Permissions.GetAllPermissions();

        var existingClaims = await roleManager.GetClaimsAsync(adminRole);

        foreach (var permission in permissions)
        {
            if (existingClaims.Any(x => x.Type == Permissions.Type && x.Value == permission))
                continue;

            await roleManager.AddClaimAsync(adminRole, new Claim(Permissions.Type, permission!));
        }
    }
}