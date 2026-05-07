namespace Fasally.Contracts.Tailors;

public class TailorFilterRequest
{
    public int?    CategoryId        { get; set; }
    public double? MinRating         { get; set; }
    public int?    MinExperienceYears { get; set; }
    public string? Search            { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize   { get; set; } = 10;
}
