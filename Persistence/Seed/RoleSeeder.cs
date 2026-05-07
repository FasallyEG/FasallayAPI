using Fasally.Abstractions.Consts;
using Fasally.Entities;
using Microsoft.AspNetCore.Identity;

namespace Fasally.Persistence.Seed;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        string[] roles = [DefaultRoles.Admin, DefaultRoles.Member, DefaultRoles.Tailor];

        foreach (var roleName in roles)
        {
            if (await roleManager.FindByNameAsync(roleName) is not null)
                continue;

            await roleManager.CreateAsync(new ApplicationRole
            {
                Name           = roleName,
                NormalizedName = roleName.ToUpper(),
                IsDefault      = roleName == DefaultRoles.Member
            });
        }
    }
}
