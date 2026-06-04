namespace Fasally.Entities;

public class ProjectMeasurement
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = default!;

    public double? Chest { get; set; }
    public double? Waist { get; set; }
    public double? Hip { get; set; }
    public double? Length { get; set; }
    public double? Sleeve { get; set; }
    public string? AdditionalMeasurements { get; set; }
}
