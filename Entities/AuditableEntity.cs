namespace Fasally.Entities;
public abstract class AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? UpdatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
}
