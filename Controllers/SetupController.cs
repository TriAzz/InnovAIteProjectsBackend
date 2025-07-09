using Microsoft.AspNetCore.Mvc;
using innovaite_projects_dashboard.Models;
using innovaite_projects_dashboard.Persistence;
using innovaite_projects_dashboard.Authentication;

namespace innovaite_projects_dashboard.Controllers;

[ApiController]
[Route("api/setup")]
public class SetupController : ControllerBase
{
    private readonly IUserDataAccess _userRepo;
    private readonly DashboardContext _context;

    public SetupController(IUserDataAccess userRepo, DashboardContext context)
    {
        _userRepo = userRepo;
        _context = context;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        try
        {
            var users = await _userRepo.GetUsersAsync();
            return Ok(new
            {
                status = "online",
                databaseConnected = true,
                usersCount = users.Count,
                adminUserExists = users.Any(u => u.Role == "Admin"),
                databaseName = _context.GetDatabaseName()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "error",
                databaseConnected = false,
                error = ex.Message,
                innerError = ex.InnerException?.Message
            });
        }
    }

    [HttpPost("first-admin")]
    public async Task<IActionResult> CreateFirstAdmin([FromBody] UserRegistrationRequest request)
    {
        if (request == null)
        {
            return BadRequest("Admin data is required");
        }

        try
        {
            // Check if any users already exist
            var existingUsers = await _userRepo.GetUsersAsync();
            if (existingUsers.Count > 0)
            {
                return Conflict(new { message = "Users already exist in the system", usersCount = existingUsers.Count });
            }

            // Create the admin user
            var newUser = new User
            {
                FirstName = request.FirstName ?? "Admin",
                LastName = request.LastName ?? "User",
                Email = request.Email,
                PasswordHash = Argon2PasswordHasher.HashPassword(request.Password),
                Role = "Admin",
                Description = request.Description ?? "Initial admin user"
            };

            await _userRepo.AddUserAsync(newUser);

            return Created($"/api/users/{newUser.Id}", new 
            { 
                message = "Admin user created successfully",
                userId = newUser.Id,
                email = newUser.Email,
                role = newUser.Role
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
