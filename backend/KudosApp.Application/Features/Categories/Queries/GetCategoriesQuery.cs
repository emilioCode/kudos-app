using MediatR;
using KudosApp.Application.Features.Categories.DTOs;

namespace KudosApp.Application.Features.Categories.Queries;

public record GetCategoriesQuery() : IRequest<List<CategoryResponse>>;
