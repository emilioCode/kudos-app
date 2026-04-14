using Microsoft.EntityFrameworkCore;
using KudosApp.Domain.Entities;
using KudosApp.Domain.Enums;

namespace KudosApp.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Kudos> Kudos => Set<Kudos>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<UserBadge> UserBadges => Set<UserBadge>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // UserBadge composite key
        modelBuilder.Entity<UserBadge>()
            .HasKey(ub => new { ub.UserId, ub.BadgeId });

        // Kudos Sender relationship
        modelBuilder.Entity<Kudos>()
            .HasOne(k => k.Sender)
            .WithMany(u => u.SentKudos)
            .HasForeignKey(k => k.SenderId)
            .OnDelete(DeleteBehavior.NoAction);

        // Kudos Recipient relationship
        modelBuilder.Entity<Kudos>()
            .HasOne(k => k.Recipient)
            .WithMany(u => u.ReceivedKudos)
            .HasForeignKey(k => k.RecipientId)
            .OnDelete(DeleteBehavior.NoAction);

        // Kudos Category relationship
        modelBuilder.Entity<Kudos>()
            .HasOne(k => k.Category)
            .WithMany(c => c.Kudos)
            .HasForeignKey(k => k.CategoryId);

        // User.Role as string
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        // All Guid IDs use ValueGeneratedOnAdd
        modelBuilder.Entity<User>()
            .Property(u => u.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Kudos>()
            .Property(k => k.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Category>()
            .Property(c => c.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Badge>()
            .Property(b => b.Id)
            .ValueGeneratedOnAdd();

        // Seed Categories with fixed Guids
        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Teamwork",
                Description = "Recognizes outstanding collaboration.",
                Points = 10
            },
            new Category
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Innovation",
                Description = "Rewards creative and innovative solutions.",
                Points = 15
            },
            new Category
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Leadership",
                Description = "Honors exceptional leadership.",
                Points = 20
            },
            new Category
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Helpfulness",
                Description = "Appreciates those who help others.",
                Points = 10
            },
            new Category
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Name = "Excellence",
                Description = "Celebrates excellent performance.",
                Points = 20
            }
        );
    }
}
