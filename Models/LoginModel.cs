using System.ComponentModel.DataAnnotations;

namespace innovaite_projects_dashboard.Models;

public class LoginModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}
