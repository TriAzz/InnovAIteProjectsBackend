using System.ComponentModel.DataAnnotations;

namespace innovaite_projects_dashboard.Models;

public class CreateProjectRequest
{
    [Required]
    public string Title { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    public string? GitHubUrl { get; set; }

    public string? LiveSiteUrl { get; set; }

    public string Status { get; set; } = "Not Started"; // Not Started, In Progress, Completed

    public string? Tools { get; set; }
}
