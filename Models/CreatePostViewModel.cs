using System.ComponentModel.DataAnnotations;

namespace event_web_dev_project.Models;

public class CreatePostViewModel
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Location { get; set; } = string.Empty;

    [Range(1, 100)]
    public int MaxMembers { get; set; }

    [Required]
    public string Deadline { get; set; } = string.Empty;  // comes as "yyyy-MM-ddTHH:mm" from JS

    [Required]
    public string ActivityDate { get; set; } = string.Empty;  // comes as "yyyy-MM-ddTHH:mm" from JS, must be >= Deadline

    [Required]
    public string Mode { get; set; } = "fifo";
}
