using FluentAssertions;
using KudosApp.Application.Features.Auth.Commands;
using KudosApp.Application.Interfaces;
using KudosApp.Domain.Entities;
using KudosApp.Domain.Enums;
using KudosApp.Infrastructure.Data;
using KudosApp.Infrastructure.Features.Auth.Handlers;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace KudosApp.Tests.Features.Handlers.Auth;

public class LoginHandlerTests
{
    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAuthResponse()
    {
        var db = CreateDbContext(nameof(Login_WithValidCredentials_ReturnsAuthResponse));
        var password = "Password123!";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.User,
            Points = 0,
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(t => t.GenerateToken(It.IsAny<User>())).Returns("token");

        var handler = new LoginHandler(db, tokenService.Object);

        var command = new LoginCommand("test@example.com", password);

        var response = await handler.Handle(command, default);

        response.Should().NotBeNull();
        response.Token.Should().Be("token");
        response.Name.Should().Be("Test User");
        response.Email.Should().Be("test@example.com");
        response.Role.Should().Be(UserRole.User.ToString());
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ThrowsUnauthorizedAccessException()
    {
        var db = CreateDbContext(nameof(Login_WithInvalidEmail_ThrowsUnauthorizedAccessException));
        var tokenService = new Mock<ITokenService>();
        var handler = new LoginHandler(db, tokenService.Object);

        var command = new LoginCommand("notfound@example.com", "Password123!");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, default));
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ThrowsUnauthorizedAccessException()
    {
        var db = CreateDbContext(nameof(Login_WithInvalidPassword_ThrowsUnauthorizedAccessException));
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword"),
            Role = UserRole.User,
            Points = 0,
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var tokenService = new Mock<ITokenService>();
        var handler = new LoginHandler(db, tokenService.Object);

        var command = new LoginCommand("test@example.com", "WrongPassword");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, default));
    }
}
