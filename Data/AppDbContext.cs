using Microsoft.AspNetCore.Identity.EntityFrameworkCore;  // ← this was missing!
using Microsoft.EntityFrameworkCore;
using event_web_dev_project.Models;

namespace event_web_dev_project.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>  // ← only one class
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
                PostedBy = "Alex Johnson",
                PostedAt = new DateTime(2026, 2, 11, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<PostApplication>().HasData(
            new PostApplication { Id = 1, PostId = 1, ApplicantName = "Sarah Chen",     Message = "I'd love to join! I play midfielder and have experience.", Status = "Accepted", AppliedAt = new DateTime(2026, 2, 11, 14, 30, 0, DateTimeKind.Utc) },
            new PostApplication { Id = 2, PostId = 1, ApplicantName = "Mike Rodriguez", Message = "Count me in! I'm available on Sunday.",                    Status = "Accepted", AppliedAt = new DateTime(2026, 2, 11, 15, 0,  0, DateTimeKind.Utc) },
            new PostApplication { Id = 3, PostId = 1, ApplicantName = "Emily Park",     Message = "I'm interested! Can I bring a friend?",                    Status = "Pending",  AppliedAt = new DateTime(2026, 2, 11, 16, 0,  0, DateTimeKind.Utc) },
            new PostApplication { Id = 4, PostId = 1, ApplicantName = "Jessica Liu",    Message = "Would love to join but I'm a beginner. Is that okay?",     Status = "Rejected", AppliedAt = new DateTime(2026, 2, 11, 17, 0,  0, DateTimeKind.Utc) }
        );
    }
}