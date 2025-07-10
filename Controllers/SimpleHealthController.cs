using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;

namespace innovaite_projects_dashboard.Controllers;

[ApiController]
[Route("/health")]
[EnableCors("AllowAll")]
public class SimpleHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new {
            status = "online",
            timestamp = DateTime.UtcNow,
            message = "API is running",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            cors = "AllowAll policy applied"
        });
    }
}
