using MediatR;
using KudosApp.Application.Features.Auth.Commands;
using KudosApp.Application.Features.Auth.DTOs;
using KudosApp.Infrastructure.Data;
using KudosApp.Domain.Entities;
using KudosApp.Domain.Enums;
using KudosApp.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace KudosApp.Infrastructure.Features.Auth.Handlers;

public class RegisterHandler(AppDbContext db, ITokenService tokenService) : IRequestHandler<RegisterCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
            throw new InvalidOperationException("Email already exists.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.User,
            Points = 0,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

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
