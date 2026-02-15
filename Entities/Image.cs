using Fasally.Entities;

namespace Fasally.Entities;
public class Image : AuditableEntity
{
    public string FileName { get; set; } = null!;
    public string OriginalName { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public long Size { get; set; }
}
