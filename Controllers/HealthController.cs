using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using System.Collections.Generic;
using System.Threading.Tasks;
using innovaite_projects_dashboard.Persistence;
using MongoDB.Driver;

namespace innovaite_projects_dashboard.Controllers;

/// <summary>
/// API health check controller
/// </summary>
[ApiController]
[Route("api/health")]
[EnableCors("AllowAll")]
public class HealthController : ControllerBase
{
    private readonly IUserDataAccess _userRepo;
    private readonly DashboardContext _context;

    /// <summary>
    /// Constructor
    /// </summary>
    public HealthController(IUserDataAccess userRepo, DashboardContext context)
    {
        _userRepo = userRepo;
        _context = context;
    }

    /// <summary>
    /// Gets health status of the API
    /// </summary>
    /// <returns>Health status information</returns>
    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        var healthInfo = new Dictionary<string, object>
        {
            ["status"] = "online",
            ["timestamp"] = System.DateTime.UtcNow,
            ["databaseConnection"] = "checking..."
        };

        try
        {
            // Check MongoDB connection
            var userCount = await _userRepo.GetUsersAsync();
            healthInfo["databaseConnection"] = "connected";
            healthInfo["userCount"] = userCount.Count;
            
            // Get database name
            var databaseName = _context.GetDatabaseName();
            healthInfo["databaseName"] = databaseName;

            return Ok(healthInfo);
        }
        catch (Exception ex)
        {
            // Add detailed error information but still return 200 OK
            // This makes debugging easier while keeping the endpoint accessible
            healthInfo["databaseConnection"] = "error";
            healthInfo["errorType"] = ex.GetType().Name;
            healthInfo["errorMessage"] = ex.Message;
            healthInfo["innerErrorMessage"] = ex.InnerException?.Message;
            
            // Add environment info to help with debugging
            healthInfo["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown";
            healthInfo["hasMongoDbEnvVar"] = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING"));
            
            // Log the error
            Console.WriteLine($"[ERROR] Health check failed: {ex.Message}");
            
            // Return 200 OK for monitoring tools, with error details in the response body
            return Ok(healthInfo);
        }
    }
}
