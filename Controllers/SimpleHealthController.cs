using Microsoft.AspNetCore.Mvc;

namespace innovaite_projects_dashboard.Controllers;

[ApiController]
[Route("/health")]
public class SimpleHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new {
            status = "online",
            timestamp = DateTime.UtcNow,
            message = "API is running",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
        });
    }
}
