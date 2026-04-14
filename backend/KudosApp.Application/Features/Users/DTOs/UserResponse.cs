namespace KudosApp.Application.Features.Users.DTOs;

public class UserResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
    public int Points { get; set; }
}
