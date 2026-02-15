namespace Fasally.Entities;

public class RefreshToken : AuditableEntity
{
    public string Token { get; set; } = null!;
    public DateTime ExpiresOn { get; set; }
    public DateTime? RevokedOn { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
    public bool IsActive => RevokedOn is null && !IsExpired;
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
