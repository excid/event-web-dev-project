using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace event_web_dev_project.Models;

public class Review
{
    [Key]
    public int Id { get; set; }

    // The post this review belongs to
    public int PostId { get; set; }

    // The user who wrote the review
    public string ReviewerId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ReviewerName { get; set; } = string.Empty;

    // The user being reviewed
    public string RevieweeId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string RevieweeName { get; set; } = string.Empty;

    // 1–5 star rating
    public int Rating { get; set; }

    public string? Comment { get; set; }

    public bool IsAnonymous { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
