using MediatR;
using KudosApp.Application.Features.Leaderboard.DTOs;

namespace KudosApp.Application.Features.Leaderboard.Queries;

public record GetLeaderboardQuery() : IRequest<List<LeaderboardResponse>>;
