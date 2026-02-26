using System.ComponentModel.DataAnnotations;

namespace event_web_dev_project.Models;

public class ActivityPost
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    public int MaxMembers { get; set; }

    public int CurrentMembers { get; set; } = 0;

    public DateTime ExpiresAt { get; set; }

    [MaxLength(100)]
    public string ApplicationMode { get; set; } = "Owner selects";

    // "Open" or "Closed"
    [MaxLength(20)]
    public string Status { get; set; } = "Open";

    [MaxLength(100)]
    public string PostedBy { get; set; } = string.Empty;

    public DateTime PostedAt { get; set; } = DateTime.Now;

    public string? OwnerId { get; set; }

    // --- Soft Delete ---
    // When true, this post is hidden from all listings.
    // It stays in the database so you can recover it if needed.
    // Set to true when the post is closed.
    public bool IsDeleted { get; set; } = false;

    // When it was soft-deleted (useful for auto-cleanup later)
    public DateTime? DeletedAt { get; set; } = null;

    // Navigation property
    public List<PostApplication> Applications { get; set; } = new();

    // Computed helper (not stored in DB)
    public int SpotsLeft => MaxMembers - CurrentMembers;
}
