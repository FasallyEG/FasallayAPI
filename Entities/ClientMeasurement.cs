namespace Fasally.Entities;

public class ClientMeasurement
{
    public string ApplicationUserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;

    public double? Chest { get; set; }
    public double? Waist { get; set; }
    public double? Hip { get; set; }
    public double? Length { get; set; }
    public double? Sleeve { get; set; }
    public string? AdditionalMeasurements { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
