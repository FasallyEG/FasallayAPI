using Fasally.Abstractions.Consts;
using Fasally.Entities;
using Microsoft.AspNetCore.Identity;

namespace Fasally.Persistence.Seed;

public static class UserSeeder
{
    public static async Task SeedUsersAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var admin = await userManager.FindByEmailAsync(
            DefaultUsers.Admin.Email);

        if (admin is not null)
            return;

        admin = new ApplicationUser
        {
            UserName = DefaultUsers.Admin.Email,
            Email = DefaultUsers.Admin.Email,
            EmailConfirmed = true,
            FirstName = "System",
            LastName = "Admin"
        };

        await userManager.CreateAsync(
            admin,
            DefaultUsers.Admin.Password);
    }
}