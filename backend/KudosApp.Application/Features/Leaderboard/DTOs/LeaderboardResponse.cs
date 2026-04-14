namespace KudosApp.Application.Features.Leaderboard.DTOs;

public class LeaderboardResponse
{
    public Guid UserId { get; set; }
    public required string Name { get; set; }
    public int Points { get; set; }
    public int BadgeCount { get; set; }
    public int Rank { get; set; }
}
