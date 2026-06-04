using Fasally.Entities.Enums;

namespace Fasally.Entities;

public class Tailor
{
    // PK = FK (1:1 with ApplicationUser)
    public string ApplicationUserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;

    public int ExperienceYears { get; set; }
    public string? Bio { get; set; }

    public string? NationalIdImageUrl { get; set; }
    public string? ShopImageUrl { get; set; }

    // Verification — role is assigned only when Approved
    public ProfileStatus Status { get; set; } = ProfileStatus.Pending;
    public bool IsVerified => Status == ProfileStatus.Approved;

    // Metrics (updated by background logic / reviews later)
    public double ResponseRate { get; set; } = 0;
    public double AverageRating { get; set; } = 0;
    public int TotalReviews { get; set; } = 0;

    // Navigation
    public ICollection<PortfolioItem> PortfolioItems { get; set; } = [];
    public ICollection<Category> Categories { get; set; } = [];
    public ICollection<Proposal> Proposals { get; set; } = [];
}
