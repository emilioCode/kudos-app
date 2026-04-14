using MediatR;
using KudosApp.Application.Features.Leaderboard.Queries;
using KudosApp.Application.Features.Leaderboard.DTOs;
using KudosApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KudosApp.Infrastructure.Features.Leaderboard.Handlers;

public class GetLeaderboardHandler(AppDbContext db) : IRequestHandler<GetLeaderboardQuery, List<LeaderboardResponse>>
{
    public async Task<List<LeaderboardResponse>> Handle(GetLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var users = await db.Users
            .Include(u => u.UserBadges)
            .OrderByDescending(u => u.Points)
            .ToListAsync(cancellationToken);

        var leaderboard = users
            .Select((u, i) => new LeaderboardResponse
            {
                UserId = u.Id,
                Name = u.Name,
                Points = u.Points,
                BadgeCount = u.UserBadges.Count,
                Rank = i + 1
            })
            .ToList();

        return leaderboard;
    }
}
