namespace event_web_dev_project.Models;

public class UserCardModel
{
    public string  Id          { get; set; } = string.Empty;
    public string? ProfileSlug { get; set; }
    public string  DisplayName { get; set; } = string.Empty;
    public string? About       { get; set; }
    public string? AvatarUrl   { get; set; }
    public string? Tags        { get; set; }
    public string? Interests   { get; set; }
}