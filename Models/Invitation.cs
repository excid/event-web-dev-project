using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace event_web_dev_project.Models;

public class Invitation
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string SenderId { get; set; } = string.Empty;

    [Required]
    public string ReceiverId { get; set; } = string.Empty;

    // The ActivityPost this invitation is for
    [ForeignKey("Post")]
    public int PostId { get; set; }

    // "Pending", "Accepted", "Rejected"
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public string? Message { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("SenderId")]
    public ApplicationUser? Sender { get; set; }

    [ForeignKey("ReceiverId")]
    public ApplicationUser? Receiver { get; set; }

    public ActivityPost? Post { get; set; }
}
