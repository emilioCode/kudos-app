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

public class RegisterHandlerTests
{
    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsAuthResponse()
    {
        var db = CreateDbContext(nameof(Register_WithValidData_ReturnsAuthResponse));
        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(t => t.GenerateToken(It.IsAny<User>())).Returns("token");

        var handler = new RegisterHandler(db, tokenService.Object);

        var command = new RegisterCommand("Test User", "test@example.com", "Password123!");

        var response = await handler.Handle(command, default);

        response.Role.Should().Be("User");
        response.Token.Should().Be("token");
        response.Name.Should().Be("Test User");
        response.Email.Should().Be("test@example.com");
        response.Role.Should().Be(UserRole.User.ToString());
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        var db = CreateDbContext(nameof(Register_WithDuplicateEmail_ThrowsInvalidOperationException));
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Name = "Existing",
            Email = "duplicate@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 0,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var tokenService = new Mock<ITokenService>();
        var handler = new RegisterHandler(db, tokenService.Object);

        var command = new RegisterCommand("New User", "duplicate@example.com", "Password123!");

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, default));
    }
}
