namespace KudosApp.Domain.Entities;

public class Badge
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int PointsThreshold { get; set; }

    public List<UserBadge> UserBadges { get; set; } = new();
}
