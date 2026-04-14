using MediatR;
using KudosApp.Application.Features.Auth.Commands;
using KudosApp.Application.Features.Auth.DTOs;
using KudosApp.Infrastructure.Data;
using KudosApp.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace KudosApp.Infrastructure.Features.Auth.Handlers;

public class LoginHandler(AppDbContext db, ITokenService tokenService) : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user is null)
            throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var token = tokenService.GenerateToken(user);

        return new AuthResponse
        {
            Id = user.Id,
            Token = token,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }
}
