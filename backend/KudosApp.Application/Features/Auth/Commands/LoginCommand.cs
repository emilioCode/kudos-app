using MediatR;
using KudosApp.Application.Features.Auth.DTOs;

namespace KudosApp.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;
