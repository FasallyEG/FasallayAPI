using Fasally.Abstractions.Consts;
using Fasally.Entities;
using Microsoft.AspNetCore.Identity;

namespace Fasally.Persistence.Seed;

public static class RoleSeeder
    {
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        if(await roleManager.FindByIdAsync(DefaultRoles.Admin.Id) is null)
            {
            await roleManager.CreateAsync(new ApplicationRole
                {
                Id = DefaultRoles.Admin.Id,
                Name = DefaultRoles.Admin.Name,
                NormalizedName = DefaultRoles.Admin.Name.ToUpper(),
                ConcurrencyStamp = DefaultRoles.Admin.ConcurrencyStamp,
                IsDefault = false
                });
            }

        if(await roleManager.FindByIdAsync(DefaultRoles.Member.Id) is null)
            {
            await roleManager.CreateAsync(new ApplicationRole
                {
                Id = DefaultRoles.Member.Id,
                Name = DefaultRoles.Member.Name,
                NormalizedName = DefaultRoles.Member.Name.ToUpper(),
                ConcurrencyStamp = DefaultRoles.Member.ConcurrencyStamp,
                IsDefault = true
                });
            }
        }
    }