using FluentAssertions;
using KudosApp.Application.Features.Users.Queries;
using KudosApp.Domain.Entities;
using KudosApp.Domain.Enums;
using KudosApp.Infrastructure.Data;
using KudosApp.Infrastructure.Features.Users.Handlers;
using Microsoft.EntityFrameworkCore;

namespace KudosApp.Tests.Features.Handlers.Users;

public class GetUsersHandlerTests
{
    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetUsers_ReturnsAllUsers()
    {
        var db = CreateDbContext(nameof(GetUsers_ReturnsAllUsers));

        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Name = "Alice Johnson",
            Email = "alice@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 50,
            CreatedAt = DateTime.UtcNow
        });
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Name = "Bob Smith",
            Email = "bob@example.com",
            PasswordHash = "hash",
            Role = UserRole.Admin,
            Points = 100,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new GetUsersHandler(db);
        var query = new GetUsersQuery();

        var result = await handler.Handle(query, default);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUsers_ReturnsOrderedByName()
    {
        var db = CreateDbContext(nameof(GetUsers_ReturnsOrderedByName));

        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Name = "Zoe Williams",
            Email = "zoe@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 30,
            CreatedAt = DateTime.UtcNow
        });
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Name = "Alice Johnson",
            Email = "alice@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 50,
            CreatedAt = DateTime.UtcNow
        });
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Name = "Mike Davis",
            Email = "mike@example.com",
            PasswordHash = "hash",
            Role = UserRole.Admin,
            Points = 75,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new GetUsersHandler(db);
        var query = new GetUsersQuery();

        var result = await handler.Handle(query, default);

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Alice Johnson");
        result[1].Name.Should().Be("Mike Davis");
        result[2].Name.Should().Be("Zoe Williams");
    }

    [Fact]
    public async Task GetUsers_ReturnsUserWithCorrectProperties()
    {
        var db = CreateDbContext(nameof(GetUsers_ReturnsUserWithCorrectProperties));

        var userId = Guid.NewGuid();
        db.Users.Add(new User
        {
            Id = userId,
            Name = "John Doe",
            Email = "john@example.com",
            PasswordHash = "hash",
            Role = UserRole.Admin,
            Points = 120,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new GetUsersHandler(db);
        var query = new GetUsersQuery();

        var result = await handler.Handle(query, default);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(userId);
        result[0].Name.Should().Be("John Doe");
        result[0].Email.Should().Be("john@example.com");
        result[0].Role.Should().Be(UserRole.Admin.ToString());
        result[0].Points.Should().Be(120);
    }

    [Fact]
    public async Task GetUsers_WithNoUsers_ReturnsEmptyList()
    {
        var db = CreateDbContext(nameof(GetUsers_WithNoUsers_ReturnsEmptyList));

        var handler = new GetUsersHandler(db);
        var query = new GetUsersQuery();

        var result = await handler.Handle(query, default);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUsers_ReturnsUsersWithDifferentRoles()
    {
        var db = CreateDbContext(nameof(GetUsers_ReturnsUsersWithDifferentRoles));

        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Name = "Admin User",
            Email = "admin@example.com",
            PasswordHash = "hash",
            Role = UserRole.Admin,
            Points = 200,
            CreatedAt = DateTime.UtcNow
        });
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Name = "Regular User",
            Email = "user@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 50,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new GetUsersHandler(db);
        var query = new GetUsersQuery();

        var result = await handler.Handle(query, default);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Role == UserRole.Admin.ToString());
        result.Should().Contain(u => u.Role == UserRole.User.ToString());
    }
}