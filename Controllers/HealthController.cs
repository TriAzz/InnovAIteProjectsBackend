using Microsoft.AspNetCore.Mvc;
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
        catch (MongoException ex)
        {
            healthInfo["databaseConnection"] = "error";
            healthInfo["errorMessage"] = ex.Message;
            return StatusCode(500, healthInfo);
        }
    }
}
