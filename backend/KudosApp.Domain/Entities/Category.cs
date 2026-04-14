namespace KudosApp.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Points { get; set; }

    public List<Kudos> Kudos { get; set; } = new();
}
