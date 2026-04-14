using FluentAssertions;
using KudosApp.Application.Features.Leaderboard.Queries;
using KudosApp.Domain.Entities;
using KudosApp.Domain.Enums;
using KudosApp.Infrastructure.Data;
using KudosApp.Infrastructure.Features.Leaderboard.Handlers;
using Microsoft.EntityFrameworkCore;

namespace KudosApp.Tests.Features.Handlers.Leaderboard;

public class GetLeaderboardHandlerTests
{
    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetLeaderboard_ReturnsAllUsers()
    {
        var db = CreateDbContext(nameof(GetLeaderboard_ReturnsAllUsers));

        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Name = "Alice Johnson",
            Email = "alice@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 100,
            CreatedAt = DateTime.UtcNow
        });
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Name = "Bob Smith",
            Email = "bob@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 50,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new GetLeaderboardHandler(db);
        var query = new GetLeaderboardQuery();

        var result = await handler.Handle(query, default);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetLeaderboard_ReturnsOrderedByPointsDescending()
    {
        var db = CreateDbContext(nameof(GetLeaderboard_ReturnsOrderedByPointsDescending));

        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Name = "Low Points User",
            Email = "low@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 20,
            CreatedAt = DateTime.UtcNow
        });
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Name = "High Points User",
            Email = "high@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 150,
            CreatedAt = DateTime.UtcNow
        });
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Name = "Medium Points User",
            Email = "medium@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 75,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new GetLeaderboardHandler(db);
        var query = new GetLeaderboardQuery();

        var result = await handler.Handle(query, default);

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("High Points User");
        result[0].Points.Should().Be(150);
        result[1].Name.Should().Be("Medium Points User");
        result[1].Points.Should().Be(75);
        result[2].Name.Should().Be("Low Points User");
        result[2].Points.Should().Be(20);
    }

    [Fact]
    public async Task GetLeaderboard_AssignsCorrectRanks()
    {
        var db = CreateDbContext(nameof(GetLeaderboard_AssignsCorrectRanks));

        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Name = "First Place",
            Email = "first@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 200,
            CreatedAt = DateTime.UtcNow
        });
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Name = "Second Place",
            Email = "second@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 150,
            CreatedAt = DateTime.UtcNow
        });
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Name = "Third Place",
            Email = "third@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 100,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new GetLeaderboardHandler(db);
        var query = new GetLeaderboardQuery();

        var result = await handler.Handle(query, default);

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].Rank.Should().Be(1);
        result[1].Rank.Should().Be(2);
        result[2].Rank.Should().Be(3);
    }

    [Fact]
    public async Task GetLeaderboard_IncludesBadgeCount()
    {
        var db = CreateDbContext(nameof(GetLeaderboard_IncludesBadgeCount));

        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var badge1 = Guid.NewGuid();
        var badge2 = Guid.NewGuid();

        db.Users.Add(new User
        {
            Id = userId1,
            Name = "User With Badges",
            Email = "withbadges@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 100,
            CreatedAt = DateTime.UtcNow
        });
        db.Users.Add(new User
        {
            Id = userId2,
            Name = "User Without Badges",
            Email = "nobadges@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 50,
            CreatedAt = DateTime.UtcNow
        });
        db.Badges.Add(new Badge
        {
            Id = badge1,
            Name = "Bronze Badge",
            Description = "First badge",
            Icon = "bronze.png",
            PointsThreshold = 50
        });
        db.Badges.Add(new Badge
        {
            Id = badge2,
            Name = "Silver Badge",
            Description = "Second badge",
            Icon = "silver.png",
            PointsThreshold = 100
        });
        db.UserBadges.Add(new UserBadge
        {
            UserId = userId1,
            BadgeId = badge1,
            EarnedAt = DateTime.UtcNow
        });
        db.UserBadges.Add(new UserBadge
        {
            UserId = userId1,
            BadgeId = badge2,
            EarnedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new GetLeaderboardHandler(db);
        var query = new GetLeaderboardQuery();

        var result = await handler.Handle(query, default);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("User With Badges");
        result[0].BadgeCount.Should().Be(2);
        result[1].Name.Should().Be("User Without Badges");
        result[1].BadgeCount.Should().Be(0);
    }

    [Fact]
    public async Task GetLeaderboard_ReturnsLeaderboardWithCorrectProperties()
    {
        var db = CreateDbContext(nameof(GetLeaderboard_ReturnsLeaderboardWithCorrectProperties));

        var userId = Guid.NewGuid();
        var badgeId = Guid.NewGuid();

        db.Users.Add(new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 120,
            CreatedAt = DateTime.UtcNow
        });
        db.Badges.Add(new Badge
        {
            Id = badgeId,
            Name = "Test Badge",
            Description = "Badge description",
            Icon = "icon.png",
            PointsThreshold = 100
        });
        db.UserBadges.Add(new UserBadge
        {
            UserId = userId,
            BadgeId = badgeId,
            EarnedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new GetLeaderboardHandler(db);
        var query = new GetLeaderboardQuery();

        var result = await handler.Handle(query, default);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].UserId.Should().Be(userId);
        result[0].Name.Should().Be("Test User");
        result[0].Points.Should().Be(120);
        result[0].BadgeCount.Should().Be(1);
        result[0].Rank.Should().Be(1);
    }

    [Fact]
    public async Task GetLeaderboard_WithNoUsers_ReturnsEmptyList()
    {
        var db = CreateDbContext(nameof(GetLeaderboard_WithNoUsers_ReturnsEmptyList));

        var handler = new GetLeaderboardHandler(db);
        var query = new GetLeaderboardQuery();

        var result = await handler.Handle(query, default);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}