using KudosApp.Domain.Enums;

namespace KudosApp.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public UserRole Role { get; set; }
    public int Points { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<Kudos> SentKudos { get; set; } = new();
    public List<Kudos> ReceivedKudos { get; set; } = new();
    public List<UserBadge> UserBadges { get; set; } = new();
}
