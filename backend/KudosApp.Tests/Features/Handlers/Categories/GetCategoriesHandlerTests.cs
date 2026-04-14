using FluentAssertions;
using KudosApp.Application.Features.Categories.Queries;
using KudosApp.Domain.Entities;
using KudosApp.Infrastructure.Data;
using KudosApp.Infrastructure.Features.Categories.Handlers;
using Microsoft.EntityFrameworkCore;

namespace KudosApp.Tests.Features.Handlers.Categories;

public class GetCategoriesHandlerTests
{
    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetCategories_ReturnsAllCategories()
    {
        var db = CreateDbContext(nameof(GetCategories_ReturnsAllCategories));

        db.Categories.Add(new Category
        {
            Id = Guid.NewGuid(),
            Name = "Teamwork",
            Description = "Team collaboration",
            Points = 10
        });
        db.Categories.Add(new Category
        {
            Id = Guid.NewGuid(),
            Name = "Innovation",
            Description = "Creative solutions",
            Points = 15
        });
        await db.SaveChangesAsync();

        var handler = new GetCategoriesHandler(db);
        var query = new GetCategoriesQuery();

        var result = await handler.Handle(query, default);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCategories_ReturnsOrderedByName()
    {
        var db = CreateDbContext(nameof(GetCategories_ReturnsOrderedByName));

        db.Categories.Add(new Category
        {
            Id = Guid.NewGuid(),
            Name = "Teamwork",
            Description = "Team collaboration",
            Points = 10
        });
        db.Categories.Add(new Category
        {
            Id = Guid.NewGuid(),
            Name = "Innovation",
            Description = "Creative solutions",
            Points = 15
        });
        db.Categories.Add(new Category
        {
            Id = Guid.NewGuid(),
            Name = "Excellence",
            Description = "Outstanding performance",
            Points = 20
        });
        await db.SaveChangesAsync();

        var handler = new GetCategoriesHandler(db);
        var query = new GetCategoriesQuery();

        var result = await handler.Handle(query, default);

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Excellence");
        result[1].Name.Should().Be("Innovation");
        result[2].Name.Should().Be("Teamwork");
    }

    [Fact]
    public async Task GetCategories_ReturnsCategoryWithCorrectProperties()
    {
        var db = CreateDbContext(nameof(GetCategories_ReturnsCategoryWithCorrectProperties));

        var categoryId = Guid.NewGuid();
        db.Categories.Add(new Category
        {
            Id = categoryId,
            Name = "Leadership",
            Description = "Shows great leadership skills",
            Points = 20
        });
        await db.SaveChangesAsync();

        var handler = new GetCategoriesHandler(db);
        var query = new GetCategoriesQuery();

        var result = await handler.Handle(query, default);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(categoryId);
        result[0].Name.Should().Be("Leadership");
        result[0].Description.Should().Be("Shows great leadership skills");
        result[0].Points.Should().Be(20);
    }

    [Fact]
    public async Task GetCategories_WithNoCategories_ReturnsEmptyList()
    {
        var db = CreateDbContext(nameof(GetCategories_WithNoCategories_ReturnsEmptyList));

        var handler = new GetCategoriesHandler(db);
        var query = new GetCategoriesQuery();

        var result = await handler.Handle(query, default);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}