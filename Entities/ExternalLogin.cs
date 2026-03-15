using Fasally.Entities;

namespace Fasally.Entities;
public class ExternalLogin
{
    public int Id { get; set; }

    public string Provider { get; set; } = default!;

    public string ProviderUserId { get; set; } = default!;

    public string UserId { get; set; } = default!;

    public ApplicationUser User { get; set; } = default!;
}