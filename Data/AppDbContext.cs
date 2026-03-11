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
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Invitation> Invitations { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Invitation>()
            .HasOne(i => i.Sender)
            .WithMany()
            .HasForeignKey(i => i.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Invitation>()
            .HasOne(i => i.Receiver)
            .WithMany()
            .HasForeignKey(i => i.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}