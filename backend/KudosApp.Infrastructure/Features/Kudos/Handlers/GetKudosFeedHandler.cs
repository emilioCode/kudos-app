using MediatR;
using KudosApp.Application.Features.Kudos.Queries;
using KudosApp.Application.Features.Kudos.DTOs;
using KudosApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KudosApp.Infrastructure.Features.Kudos.Handlers;

public class GetKudosFeedHandler(AppDbContext db) : IRequestHandler<GetKudosFeedQuery, List<KudosResponse>>
{
    public async Task<List<KudosResponse>> Handle(GetKudosFeedQuery request, CancellationToken cancellationToken)
    {
        var kudosList = await db.Kudos
            .Include(k => k.Sender)
            .Include(k => k.Recipient)
            .Include(k => k.Category)
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new KudosResponse
            {
                Id = k.Id,
                SenderName = k.Sender != null ? k.Sender.Name : string.Empty,
                RecipientName = k.Recipient != null ? k.Recipient.Name : string.Empty,
                CategoryName = k.Category != null ? k.Category.Name : string.Empty,
                Message = k.Message,
                Points = k.Category != null ? k.Category.Points : 0,
                CreatedAt = k.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return kudosList;
    }
}
