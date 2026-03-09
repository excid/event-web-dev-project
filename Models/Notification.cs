using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace event_web_dev_project.Models;

public class Notification
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// Notification type: InvitationReceived, InvitationAccepted, InvitationRejected,
    /// ApplicationReceived, ApplicationAccepted, ApplicationRejected, ReviewReceived
    /// </summary>
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(400)]
    public string Message { get; set; } = string.Empty;

    /// <summary>Optional URL to navigate to when the user clicks the notification.</summary>
    public string? ActionUrl { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
