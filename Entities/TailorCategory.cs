namespace Fasally.Entities;

public class TailorCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation
    public ICollection<Tailor> Tailors { get; set; } = [];
}
