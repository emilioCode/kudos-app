using MediatR;
using KudosApp.Application.Features.Kudos.Commands;
using KudosApp.Application.Features.Kudos.DTOs;
using KudosApp.Infrastructure.Data;
using KudosApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KudosApp.Infrastructure.Features.Kudos.Handlers;

public class GiveKudosHandler(AppDbContext db) : IRequestHandler<GiveKudosCommand, KudosResponse>
{
    public async Task<KudosResponse> Handle(GiveKudosCommand request, CancellationToken cancellationToken)
    {
        var sender = await db.Users.FindAsync(new object[] { request.SenderId }, cancellationToken);
        var recipient = await db.Users.Include(u => u.UserBadges).FirstOrDefaultAsync(u => u.Id == request.RecipientId, cancellationToken);
        var category = await db.Categories.FindAsync(new object[] { request.CategoryId }, cancellationToken);

        if (sender is null)
            throw new InvalidOperationException("Sender not found.");
        if (recipient is null)
            throw new InvalidOperationException("Recipient not found.");
        if (category is null)
            throw new InvalidOperationException("Category not found.");

        var kudos = new Domain.Entities.Kudos
        {
            Id = Guid.NewGuid(),
            SenderId = sender.Id,
            RecipientId = recipient.Id,
            CategoryId = category.Id,
            Message = request.Message,
            CreatedAt = DateTime.UtcNow
        };

        db.Kudos.Add(kudos);

        recipient.Points += category.Points;

        // Award badges if thresholds are met
        var badges = await db.Badges.ToListAsync(cancellationToken);
        foreach (var badge in badges)
        {
            var alreadyAwarded = recipient.UserBadges.Any(ub => ub.BadgeId == badge.Id);
            if (!alreadyAwarded && recipient.Points >= badge.PointsThreshold)
            {
                db.UserBadges.Add(new UserBadge
                {
                    UserId = recipient.Id,
                    BadgeId = badge.Id,
                    EarnedAt = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        return new KudosResponse
        {
            Id = kudos.Id,
            SenderName = sender.Name,
            RecipientName = recipient.Name,
            CategoryName = category.Name,
            Message = kudos.Message,
            Points = category.Points,
            CreatedAt = kudos.CreatedAt
        };
    }
}
