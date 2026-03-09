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

    /// <summary>Whether the currently logged-in user is viewing their own profile.</summary>
    public bool IsOwner { get; set; } = true;

    /// <summary>Whether the currently logged-in user has already submitted a direct profile review for this user.</summary>
    public bool HasReviewedByCurrentUser { get; set; } = false;

    /// <summary>The existing direct profile review written by the current user, if any.</summary>
    public Review? ExistingReviewByCurrentUser { get; set; }

    public List<ActivityPost> post_history { get; set; } = new List<ActivityPost>();
    public List<ActivityPost> upcoming_activities { get; set; } = new List<ActivityPost>();
    public List<Review> reviews { get; set; } = new List<Review>();

    public List<string> TagList => Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
    public List<string> InterestList => Interests?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();

    public double Rating => reviews.Count > 0
        ? Math.Round(reviews.Average(r => r.Rating), 1)
        : 0.0;

    public int ReviewsCount => reviews.Count;

    public int OrganizedCount { get; set; }
    public int JoinedCount { get; set; }

    /// <summary>Open posts owned by the current (viewing) user — used to populate the Invite to Event dropdown.</summary>
    public List<ActivityPost> OwnedOpenPosts { get; set; } = new List<ActivityPost>();

    /// <summary>Post IDs for which the current user has already sent a pending invitation to this profile user.</summary>
    public HashSet<int> AlreadyInvitedPostIds { get; set; } = new HashSet<int>();
}
