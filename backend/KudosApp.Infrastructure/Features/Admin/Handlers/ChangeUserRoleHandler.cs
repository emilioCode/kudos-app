using MediatR;
using KudosApp.Application.Features.Admin.Commands;
using KudosApp.Application.Features.Users.DTOs;
using KudosApp.Infrastructure.Data;
using KudosApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace KudosApp.Infrastructure.Features.Admin.Handlers;

public class ChangeUserRoleHandler(AppDbContext db) : IRequestHandler<ChangeUserRoleCommand, UserResponse>
{
    public async Task<UserResponse> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user is null)
            throw new InvalidOperationException("User not found.");

        if (!Enum.TryParse<UserRole>(request.Role, true, out var newRole))
            throw new InvalidOperationException("Invalid role.");

        user.Role = newRole;
        await db.SaveChangesAsync(cancellationToken);

        return new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),
            Points = user.Points
        };
    }
}
