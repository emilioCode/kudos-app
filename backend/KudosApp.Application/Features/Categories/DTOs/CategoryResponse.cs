namespace KudosApp.Application.Features.Categories.DTOs;

public class CategoryResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Points { get; set; }
}
