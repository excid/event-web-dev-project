using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using event_web_dev_project.Models;

namespace event_web_dev_project.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser> 
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ActivityPost> ActivityPosts { get; set; }
    public DbSet<PostApplication> PostApplications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ActivityPost>().HasData(
            new ActivityPost
            {
                Id = 1,
                Title = "Looking for Football Teammates - Sunday Match",
                Category = "Sports",
                Description = "We need 3 more players for a friendly football match this Sunday at Central Park. All skill levels welcome! We play 7v7 format.",
                Location = "Central Park, Field 3",
                MaxMembers = 3,
                CurrentMembers = 2,
                ExpiresAt = new DateTime(2026, 2, 15, 12, 0, 0, DateTimeKind.Utc),
                ApplicationMode = "Overflow allowed - Owner selects",
                Status = "Open",
                PostedBy = "Sarah Chen",
                PostedAt = new DateTime(2026, 2, 11, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}