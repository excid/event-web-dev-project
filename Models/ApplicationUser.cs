using Microsoft.AspNetCore.Identity;

namespace event_web_dev_project.Models;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}