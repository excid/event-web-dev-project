using System.ComponentModel.DataAnnotations;

namespace event_web_dev_project.Models;

public class ProfileViewModel
{
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Display Name")]
    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? About { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Tags { get; set; } // Comma-separated for simplicity

    public string? Interests { get; set; } // Comma-separated for simplicity

    public List<ActivityPost> post_history { get; set; } = new List<ActivityPost>();

    public List<string> TagList => Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
    public List<string> InterestList => Interests?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();

    public double Rating { get; set; } = 4.5;
    public int ReviewsCount { get; set; } = 3;
    public int OrganizedCount { get; set; } = 2;
    public int JoinedCount { get; set; } = 1;
}
