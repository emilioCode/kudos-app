using MediatR;
using KudosApp.Application.Features.Users.DTOs;

namespace KudosApp.Application.Features.Admin.Queries;

public record GetAdminUsersQuery() : IRequest<List<UserResponse>>;
