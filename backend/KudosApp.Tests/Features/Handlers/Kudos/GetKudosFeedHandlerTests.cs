using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using KudosApp.Infrastructure.Data;
using KudosApp.Infrastructure.Features.Kudos.Handlers;
using KudosApp.Application.Features.Kudos.Queries;
using KudosApp.Domain.Entities;
using KudosApp.Domain.Enums;

namespace KudosApp.Tests.Features.Handlers.Kudos;

public class GetKudosFeedHandlerTests
{
    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetKudosFeed_ReturnsAllKudos()
    {
        var db = CreateDbContext(nameof(GetKudosFeed_ReturnsAllKudos));

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
            Points = 0,
            CreatedAt = DateTime.UtcNow
        });
        db.Categories.Add(new Category
        {
            Id = categoryId,
            Name = "Teamwork",
            Description = "Team collaboration",
            Points = 10
        });
        db.Kudos.Add(new Domain.Entities.Kudos
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            RecipientId = recipientId,
            CategoryId = categoryId,
            Message = "Great job!",
            CreatedAt = DateTime.UtcNow
        });
        db.Kudos.Add(new Domain.Entities.Kudos
        {
            Id = Guid.NewGuid(),
            SenderId = recipientId,
            RecipientId = senderId,
            CategoryId = categoryId,
            Message = "Thanks for your help!",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        });
        await db.SaveChangesAsync();

        var handler = new GetKudosFeedHandler(db);
        var query = new GetKudosFeedQuery();

        var result = await handler.Handle(query, default);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetKudosFeed_ReturnsOrderedByCreatedAtDescending()
    {
        var db = CreateDbContext(nameof(GetKudosFeed_ReturnsOrderedByCreatedAtDescending));

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
            Points = 0,
            CreatedAt = DateTime.UtcNow
        });
        db.Categories.Add(new Category
        {
            Id = categoryId,
            Name = "Teamwork",
            Description = "Team collaboration",
            Points = 10
        });

        var oldestKudos = new Domain.Entities.Kudos
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            RecipientId = recipientId,
            CategoryId = categoryId,
            Message = "Oldest kudos",
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };
        var newestKudos = new Domain.Entities.Kudos
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            RecipientId = recipientId,
            CategoryId = categoryId,
            Message = "Newest kudos",
            CreatedAt = DateTime.UtcNow
        };
        var middleKudos = new Domain.Entities.Kudos
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            RecipientId = recipientId,
            CategoryId = categoryId,
            Message = "Middle kudos",
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        db.Kudos.AddRange(oldestKudos, newestKudos, middleKudos);
        await db.SaveChangesAsync();

        var handler = new GetKudosFeedHandler(db);
        var query = new GetKudosFeedQuery();

        var result = await handler.Handle(query, default);

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].Message.Should().Be("Newest kudos");
        result[1].Message.Should().Be("Middle kudos");
        result[2].Message.Should().Be("Oldest kudos");
    }

    [Fact]
    public async Task GetKudosFeed_ReturnsKudosWithCorrectProperties()
    {
        var db = CreateDbContext(nameof(GetKudosFeed_ReturnsKudosWithCorrectProperties));

        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var kudosId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        db.Users.Add(new User
        {
            Id = senderId,
            Name = "John Sender",
            Email = "john@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 0,
            CreatedAt = DateTime.UtcNow
        });
        db.Users.Add(new User
        {
            Id = recipientId,
            Name = "Jane Recipient",
            Email = "jane@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 0,
            CreatedAt = DateTime.UtcNow
        });
        db.Categories.Add(new Category
        {
            Id = categoryId,
            Name = "Innovation",
            Description = "Creative solutions",
            Points = 15
        });
        db.Kudos.Add(new Domain.Entities.Kudos
        {
            Id = kudosId,
            SenderId = senderId,
            RecipientId = recipientId,
            CategoryId = categoryId,
            Message = "Amazing innovative idea!",
            CreatedAt = createdAt
        });
        await db.SaveChangesAsync();

        var handler = new GetKudosFeedHandler(db);
        var query = new GetKudosFeedQuery();

        var result = await handler.Handle(query, default);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(kudosId);
        result[0].SenderName.Should().Be("John Sender");
        result[0].RecipientName.Should().Be("Jane Recipient");
        result[0].CategoryName.Should().Be("Innovation");
        result[0].Message.Should().Be("Amazing innovative idea!");
        result[0].Points.Should().Be(15);
        result[0].CreatedAt.Should().BeCloseTo(createdAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetKudosFeed_IncludesNavigationProperties()
    {
        var db = CreateDbContext(nameof(GetKudosFeed_IncludesNavigationProperties));

        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        db.Users.Add(new User
        {
            Id = senderId,
            Name = "Sender User",
            Email = "sender@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 0,
            CreatedAt = DateTime.UtcNow
        });
        db.Users.Add(new User
        {
            Id = recipientId,
            Name = "Recipient User",
            Email = "recipient@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 0,
            CreatedAt = DateTime.UtcNow
        });
        db.Categories.Add(new Category
        {
            Id = categoryId,
            Name = "Leadership",
            Description = "Shows leadership",
            Points = 20
        });
        db.Kudos.Add(new Domain.Entities.Kudos
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            RecipientId = recipientId,
            CategoryId = categoryId,
            Message = "Excellent leadership!",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new GetKudosFeedHandler(db);
        var query = new GetKudosFeedQuery();

        var result = await handler.Handle(query, default);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].SenderName.Should().NotBeNullOrEmpty();
        result[0].RecipientName.Should().NotBeNullOrEmpty();
        result[0].CategoryName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetKudosFeed_WithNoKudos_ReturnsEmptyList()
    {
        var db = CreateDbContext(nameof(GetKudosFeed_WithNoKudos_ReturnsEmptyList));

        var handler = new GetKudosFeedHandler(db);
        var query = new GetKudosFeedQuery();

        var result = await handler.Handle(query, default);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetKudosFeed_WithMultipleCategories_ReturnsCorrectData()
    {
        var db = CreateDbContext(nameof(GetKudosFeed_WithMultipleCategories_ReturnsCorrectData));

        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var category1Id = Guid.NewGuid();
        var category2Id = Guid.NewGuid();

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
            Points = 0,
            CreatedAt = DateTime.UtcNow
        });
        db.Categories.Add(new Category
        {
            Id = category1Id,
            Name = "Teamwork",
            Description = "Team collaboration",
            Points = 10
        });
        db.Categories.Add(new Category
        {
            Id = category2Id,
            Name = "Excellence",
            Description = "Outstanding work",
            Points = 20
        });
        db.Kudos.Add(new Domain.Entities.Kudos
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            RecipientId = recipientId,
            CategoryId = category1Id,
            Message = "Great teamwork!",
            CreatedAt = DateTime.UtcNow
        });
        db.Kudos.Add(new Domain.Entities.Kudos
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            RecipientId = recipientId,
            CategoryId = category2Id,
            Message = "Excellent performance!",
            CreatedAt = DateTime.UtcNow.AddMinutes(-1)
        });
        await db.SaveChangesAsync();

        var handler = new GetKudosFeedHandler(db);
        var query = new GetKudosFeedQuery();

        var result = await handler.Handle(query, default);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(k => k.CategoryName == "Teamwork" && k.Points == 10);
        result.Should().Contain(k => k.CategoryName == "Excellence" && k.Points == 20);
    }
}