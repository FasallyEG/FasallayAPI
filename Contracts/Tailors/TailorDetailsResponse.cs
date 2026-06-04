namespace Fasally.Contracts.Tailors;

public class TailorDetailsResponse
{
    public string  Id              { get; set; } = null!;
    public string  FullName        { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
    public string? Bio             { get; set; }
    public int     ExperienceYears { get; set; }
    public double  AverageRating   { get; set; }
    public double  ResponseRate    { get; set; }
    public int     TotalReviews    { get; set; }

    public IEnumerable<string>                Categories { get; set; } = new List<string>();
    public IEnumerable<PortfolioItemResponse> Portfolio  { get; set; } = new List<PortfolioItemResponse>();
}
