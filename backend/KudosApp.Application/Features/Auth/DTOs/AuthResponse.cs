namespace KudosApp.Application.Features.Auth.DTOs;

public class AuthResponse
{
    public Guid Id { get; set; }
    public required string Token { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
}
