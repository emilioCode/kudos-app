using FluentAssertions;
using KudosApp.Application.Features.Admin.Commands;
using KudosApp.Application.Features.Admin.Queries;
using KudosApp.Domain.Entities;
using KudosApp.Domain.Enums;
using KudosApp.Infrastructure.Data;
using KudosApp.Infrastructure.Features.Admin.Handlers;
using Microsoft.EntityFrameworkCore;

namespace KudosApp.Tests.Features.Handlers.Admin;

public class AdminHandlerTests
{
    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetAdminUsers_ReturnsAllUsers()
    {
        var db = CreateDbContext(nameof(GetAdminUsers_ReturnsAllUsers));

        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Name = "User One",
            Email = "user1@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 10,
            CreatedAt = DateTime.UtcNow
        });
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Name = "User Two",
            Email = "user2@example.com",
            PasswordHash = "hash",
            Role = UserRole.Admin,
            Points = 20,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new GetAdminUsersHandler(db);
        var query = new GetAdminUsersQuery();

        var result = await handler.Handle(query, default);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ChangeUserRole_WithValidUser_UpdatesRoleToAdmin()
    {
        var db = CreateDbContext(nameof(ChangeUserRole_WithValidUser_UpdatesRoleToAdmin));

        var userId = Guid.NewGuid();
        db.Users.Add(new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 0,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new ChangeUserRoleHandler(db);
        var command = new ChangeUserRoleCommand(userId, "Admin");

        var result = await handler.Handle(command, default);

        result.Should().NotBeNull();
        result.Role.Should().Be(UserRole.Admin.ToString());

        var updatedUser = await db.Users.FindAsync(userId);
        updatedUser!.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task GetAnalytics_ReturnsCorrectTotals()
    {
        var db = CreateDbContext(nameof(GetAnalytics_ReturnsCorrectTotals));

        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        db.Users.Add(new User
        {
            Id = senderId,
            Name = "Sender",
            Email = "sender@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 0,
            CreatedAt = DateTime.UtcNow
        });
        db.Users.Add(new User
        {
            Id = recipientId,
            Name = "Recipient",
            Email = "recipient@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 10,
            CreatedAt = DateTime.UtcNow
        });
        db.Categories.Add(new Category
        {
            Id = categoryId,
            Name = "Teamwork",
            Description = "Team collaboration",
            Points = 10
        });
        db.Kudos.Add(new KudosApp.Domain.Entities.Kudos
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            RecipientId = recipientId,
            CategoryId = categoryId,
            Message = "Great job!",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new GetAnalyticsHandler(db);
        var query = new GetAnalyticsQuery();

        var result = await handler.Handle(query, default);

        result.Should().NotBeNull();
        result.TotalUsers.Should().Be(2);
        result.TotalKudos.Should().Be(1);
        result.TotalPoints.Should().Be(10);
    }
}