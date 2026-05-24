using Fasally.Entities.Enums;

namespace Fasally.Entities;

public class SellerProfile
{
    public string ApplicationUserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;

    public string StoreName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? BusinessPhone { get; set; }
    public string? BusinessEmail { get; set; }
    public string? ShopImageUrl { get; set; }

    public ProfileStatus Status { get; set; } = ProfileStatus.Approved;
    public bool IsVerified => Status == ProfileStatus.Approved;

    public double AverageRating { get; set; } = 0;
    public int TotalReviews { get; set; } = 0;

    public ICollection<Product> Products { get; set; } = [];
}
