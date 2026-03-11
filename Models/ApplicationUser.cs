using Microsoft.AspNetCore.Identity;

namespace event_web_dev_project.Models;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    /// <summary>URL-safe unique handle used in profile URLs (e.g. /Profile?username=jane_doe).</summary>
    public string? ProfileSlug { get; set; }
    public string? About { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Tags { get; set; }
    public string? Interests { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}