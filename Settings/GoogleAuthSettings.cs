using System.ComponentModel.DataAnnotations;

namespace Fasally.Settings;
public class GoogleAuthSettings
{
    [Required]
    public string ClientId { get; set; } = string.Empty;
}