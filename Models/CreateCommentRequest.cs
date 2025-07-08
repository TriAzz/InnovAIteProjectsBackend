using System.ComponentModel.DataAnnotations;

namespace innovaite_projects_dashboard.Models;

public class CreateCommentRequest
{
    [Required]
    public string ProjectId { get; set; } = null!;

    [Required]
    public string Content { get; set; } = null!;
}
