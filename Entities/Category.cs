namespace Fasally.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation
    public ICollection<Tailor> Tailors { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}
