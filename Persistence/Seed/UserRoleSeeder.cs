using Fasally.Abstractions.Consts;
using Fasally.Entities;
using Microsoft.AspNetCore.Identity;

namespace Fasally.Persistence.Seed;

public static class UserRoleSeeder
{
    public static async Task SeedUserRolesAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        var admin = await userManager.FindByEmailAsync(DefaultUsers.Admin.Email);

        if (admin is null)
            return;

        var roleExists = await roleManager.RoleExistsAsync(DefaultRoles.Admin);

        if (!roleExists)
            return;

        var isInRole = await userManager.IsInRoleAsync(admin, DefaultRoles.Admin);

        if (!isInRole)
        {
            await userManager.AddToRoleAsync(admin, DefaultRoles.Admin);
        }
    }
}