using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;

namespace innovaite_projects_dashboard.Controllers;

[ApiController]
[Route("api/cors-test")]
[EnableCors("AllowAll")] // Permissive CORS policy for testing
public class CorsTestController : ControllerBase
{
    [HttpGet]
    public IActionResult Test()
    {
        return Ok(new {
            message = "CORS is working correctly",
            timestamp = DateTime.UtcNow,
            headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            origin = Request.Headers.Origin.ToString() ?? "No origin header"
        });
    }
    
    [HttpOptions]
    public IActionResult Options()
    {
        return Ok(new {
            message = "CORS preflight is working correctly",
            timestamp = DateTime.UtcNow
        });
    }
}
