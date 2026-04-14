using MediatR;
using KudosApp.Application.Features.Users.DTOs;

namespace KudosApp.Application.Features.Users.Queries;

public record GetUsersQuery() : IRequest<List<UserResponse>>;
