using MediatR;
using KudosApp.Application.Features.Users.Queries;
using KudosApp.Application.Features.Users.DTOs;
using KudosApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KudosApp.Infrastructure.Features.Users.Handlers;

public class GetUsersHandler(AppDbContext db) : IRequestHandler<GetUsersQuery, List<UserResponse>>
{
    public async Task<List<UserResponse>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return await db.Users
            .OrderBy(u => u.Name)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role.ToString(),
                Points = u.Points
            })
            .ToListAsync(cancellationToken);
    }
}
