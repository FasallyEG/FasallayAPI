namespace Fasally.Entities;

public class PortfolioItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Stored as JSON column (EF Core 8+)
    public List<string> ImageUrls { get; set; } = new();

    // FK to Tailor
    public string TailorId { get; set; } = default!;
    public Tailor Tailor { get; set; } = default!;
}
