using MediatR;
using KudosApp.Application.Features.Users.DTOs;

namespace KudosApp.Application.Features.Admin.Commands;

public record ChangeUserRoleCommand(Guid UserId, string Role) : IRequest<UserResponse>;
