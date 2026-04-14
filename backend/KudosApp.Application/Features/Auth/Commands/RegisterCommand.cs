using MediatR;
using KudosApp.Application.Features.Auth.DTOs;

namespace KudosApp.Application.Features.Auth.Commands;

public record RegisterCommand(string Name, string Email, string Password) : IRequest<AuthResponse>;
