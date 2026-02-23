using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace event_web_dev_project.Models;

public class PostApplication
{
    [Key]
    public int Id { get; set; }

    // Foreign key back to ActivityPost
    [ForeignKey("ActivityPost")]
    public int PostId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ApplicantName { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    // "Pending", "Accepted", "Rejected"
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public DateTime AppliedAt { get; set; } = DateTime.Now;

    // Navigation property
    public ActivityPost? ActivityPost { get; set; }
}
