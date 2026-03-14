namespace Fasally.Abstractions.Constants;

/// <summary>
/// Contains role constants for the application.
/// All role-based authorization should reference these constants.
/// </summary>
public static class Roles
{
    /// <summary>System administrator with full access to all features.</summary>
    public const string Admin = nameof(Admin);

    /// <summary>Customer who purchases clothes and uses the platform services.</summary>
    public const string Customer = nameof(Customer);

    /// <summary>Fabric seller who lists and sells fabrics on the platform.</summary>
    public const string FabricSeller = nameof(FabricSeller);

    /// <summary>Tailor who sews clothes and offers tailoring services.</summary>
    public const string Tailor = nameof(Tailor);

    /// <summary>Array of all available roles for validation and seeding.</summary>
    public static readonly string[] All = new[] { Admin, Customer, FabricSeller, Tailor };

    /// <summary>Array of roles that can self-register (excludes Admin).</summary>
    public static readonly string[] AllowedForRegistration = new[] { Customer, FabricSeller, Tailor };
}