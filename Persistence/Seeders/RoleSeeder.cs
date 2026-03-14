using Fasally.Abstractions.Constants;
using Microsoft.AspNetCore.Identity;

namespace Fasally.Persistence.Seeders;

/// <summary>
/// Responsible for seeding application roles into the database.
/// Ensures all required roles (Admin, Customer, FabricSeller, Tailor) exist.
/// </summary>
public static class RoleSeeder
{
    /// <summary>
    /// Seeds all required roles if they don't already exist.
    /// 
    /// Roles created:
    /// - Admin: System administrator with full access
    /// - Customer: Regular user who purchases clothes
    /// - FabricSeller: User who sells fabrics
    /// - Tailor: User who provides tailoring services
    /// </summary>
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        try
        {
            // Apply All 4 roles
            foreach (var roleName in Roles.All)
            {
                
                var roleExists = await roleManager.RoleExistsAsync(roleName);

                
                if (!roleExists)
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(roleName));

                    if (result.Succeeded)
                    {
                        Console.WriteLine($"Role created succesfully: {roleName}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to create the role: {roleName}");
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"   - {error.Description}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Role has been already created: {roleName}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error happened while creating roles: {ex.Message}");
            throw;
        }
    }
}