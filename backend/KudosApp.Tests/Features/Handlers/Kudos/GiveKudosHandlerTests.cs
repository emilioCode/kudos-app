using FluentAssertions;
using KudosApp.Application.Features.Kudos.Commands;
using KudosApp.Domain.Entities;
using KudosApp.Domain.Enums;
using KudosApp.Infrastructure.Data;
using KudosApp.Infrastructure.Features.Kudos.Handlers;
using Microsoft.EntityFrameworkCore;

namespace KudosApp.Tests.Features.Handlers.Kudos;

public class GiveKudosHandlerTests
{
    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GiveKudos_WithValidData_ReturnsKudosResponse()
    {
        var db = CreateDbContext(nameof(GiveKudos_WithValidData_ReturnsKudosResponse));
        var sender = new User
        {
            Id = Guid.NewGuid(),
            Name = "Sender",
            Email = "sender@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 0,
            CreatedAt = DateTime.UtcNow
        };
        var recipient = new User
        {
            Id = Guid.NewGuid(),
            Name = "Recipient",
            Email = "recipient@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 0,
            CreatedAt = DateTime.UtcNow
        };
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Teamwork",
            Description = "desc",
            Points = 10
        };
        db.Users.AddRange(sender, recipient);
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var handler = new GiveKudosHandler(db);

        var command = new GiveKudosCommand(sender.Id, recipient.Id, category.Id, "Great job!");

        var response = await handler.Handle(command, default);

        response.Should().NotBeNull();
        response.SenderName.Should().Be("Sender");
        response.RecipientName.Should().Be("Recipient");
        response.CategoryName.Should().Be("Teamwork");
        response.Message.Should().Be("Great job!");
        response.Points.Should().Be(10);
    }

    [Fact]
    public async Task GiveKudos_AddsPointsToRecipient()
    {
        var db = CreateDbContext(nameof(GiveKudos_AddsPointsToRecipient));
        var sender = new User
        {
            Id = Guid.NewGuid(),
            Name = "Sender",
            Email = "sender@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 0,
            CreatedAt = DateTime.UtcNow
        };
        var recipient = new User
        {
            Id = Guid.NewGuid(),
            Name = "Recipient",
            Email = "recipient@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 5,
            CreatedAt = DateTime.UtcNow
        };
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Teamwork",
            Description = "desc",
            Points = 10
        };
        db.Users.AddRange(sender, recipient);
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var handler = new GiveKudosHandler(db);

        var command = new GiveKudosCommand(sender.Id, recipient.Id, category.Id, "Nice work!");

        await handler.Handle(command, default);

        var updatedRecipient = await db.Users.FindAsync(recipient.Id);
        updatedRecipient!.Points.Should().Be(15);
    }

    [Fact]
    public async Task GiveKudos_WithInvalidSender_ThrowsInvalidOperationException()
    {
        var db = CreateDbContext(nameof(GiveKudos_WithInvalidSender_ThrowsInvalidOperationException));
        var recipient = new User
        {
            Id = Guid.NewGuid(),
            Name = "Recipient",
            Email = "recipient@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 0,
            CreatedAt = DateTime.UtcNow
        };
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Teamwork",
            Description = "desc",
            Points = 10
        };
        db.Users.Add(recipient);
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var handler = new GiveKudosHandler(db);

        var command = new GiveKudosCommand(Guid.NewGuid(), recipient.Id, category.Id, "Nice!");

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, default));
    }

    [Fact]
    public async Task GiveKudos_WithInvalidCategory_ThrowsInvalidOperationException()
    {
        var db = CreateDbContext(nameof(GiveKudos_WithInvalidCategory_ThrowsInvalidOperationException));
        var sender = new User
        {
            Id = Guid.NewGuid(),
            Name = "Sender",
            Email = "sender@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 0,
            CreatedAt = DateTime.UtcNow
        };
        var recipient = new User
        {
            Id = Guid.NewGuid(),
            Name = "Recipient",
            Email = "recipient@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            Points = 0,
            CreatedAt = DateTime.UtcNow
        };
        db.Users.AddRange(sender, recipient);
        await db.SaveChangesAsync();

        var handler = new GiveKudosHandler(db);

        var command = new GiveKudosCommand(sender.Id, recipient.Id, Guid.NewGuid(), "Nice!");

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, default));
    }
}
