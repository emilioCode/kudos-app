namespace KudosApp.Domain.Entities;

public class UserBadge
{
    public Guid UserId { get; set; }
    public Guid BadgeId { get; set; }
    public DateTime EarnedAt { get; set; }

    public User? User { get; set; }
    public Badge? Badge { get; set; }
}
