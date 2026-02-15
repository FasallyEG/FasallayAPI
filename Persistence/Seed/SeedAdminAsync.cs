using Fasally.Abstractions.Consts;
using Fasally.Entities;
using Microsoft.AspNetCore.Identity;

namespace Fasally.Persistence.Seed
{
    public static class UserSeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var adminId = DefaultUsers.Admin.Id;

            if (await userManager.FindByIdAsync(adminId) is not null)
                return;

            var admin = new ApplicationUser
            {
                Id = DefaultUsers.Admin.Id,
                FirstName = "Fasally",
                LastName = "Admin",
                UserName = DefaultUsers.Admin.Email,
                NormalizedUserName = DefaultUsers.Admin.Email.ToUpper(),
                Email = DefaultUsers.Admin.Email,
                NormalizedEmail = DefaultUsers.Admin.Email.ToUpper(),
                SecurityStamp = DefaultUsers.Admin.SecurityStamp,
                ConcurrencyStamp = DefaultUsers.Admin.ConcurrencyStamp,
                EmailConfirmed = true,
                IsDisabled = false
            };

            await userManager.CreateAsync(admin, "AdminPassword123!");
        }
    }
}
