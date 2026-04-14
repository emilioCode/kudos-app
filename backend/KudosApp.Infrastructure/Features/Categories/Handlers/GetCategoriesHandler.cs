using MediatR;
using KudosApp.Application.Features.Categories.Queries;
using KudosApp.Application.Features.Categories.DTOs;
using KudosApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KudosApp.Infrastructure.Features.Categories.Handlers;

public class GetCategoriesHandler(AppDbContext db) : IRequestHandler<GetCategoriesQuery, List<CategoryResponse>>
{
    public async Task<List<CategoryResponse>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Points = c.Points
            })
            .ToListAsync(cancellationToken);
    }
}
