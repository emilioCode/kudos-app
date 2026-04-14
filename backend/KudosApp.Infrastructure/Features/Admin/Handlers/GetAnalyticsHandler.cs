using MediatR;
using KudosApp.Application.Features.Admin.Queries;
using KudosApp.Application.Features.Admin.DTOs;
using KudosApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KudosApp.Infrastructure.Features.Admin.Handlers;

public class GetAnalyticsHandler(AppDbContext db) : IRequestHandler<GetAnalyticsQuery, AnalyticsResponse>
{
    public async Task<AnalyticsResponse> Handle(GetAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var totalUsers = await db.Users.CountAsync(cancellationToken);
        var totalKudos = await db.Kudos.CountAsync(cancellationToken);
        var totalPoints = await db.Users.SumAsync(u => u.Points, cancellationToken);

        var kudosByCategory = await db.Kudos
            .Include(k => k.Category)
            .GroupBy(k => k.Category!.Name)
            .Select(g => new CategoryStats
            {
                CategoryName = g.Key,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        return new AnalyticsResponse
        {
            TotalUsers = totalUsers,
            TotalKudos = totalKudos,
            TotalPoints = totalPoints,
            KudosByCategory = kudosByCategory
        };
    }
}
