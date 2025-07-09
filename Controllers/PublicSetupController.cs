using Microsoft.AspNetCore.Mvc;
using innovaite_projects_dashboard.Models;
using innovaite_projects_dashboard.Persistence;
using innovaite_projects_dashboard.Authentication;
using System;
using System.Threading.Tasks;

namespace innovaite_projects_dashboard.Controllers
{
    /// <summary>
    /// A controller for setting up the application without authentication
    /// </summary>
    [ApiController]
    [Route("api/public-setup")]
    public class PublicSetupController : ControllerBase
    {
        private readonly IUserDataAccess _userRepo;

        /// <summary>
        /// Constructor
        /// </summary>
        public PublicSetupController(IUserDataAccess userRepo)
        {
            _userRepo = userRepo;
        }

        /// <summary>
        /// Health check to verify this controller is accessible
        /// </summary>
        [HttpGet("health")]
        public IActionResult GetHealth()
        {
            return Ok(new
            {
                status = "online",
                timestamp = DateTime.UtcNow,
                message = "Public setup API is accessible"
            });
        }

        /// <summary>
        /// Creates the first admin user without authentication if no users exist
        /// </summary>
        [HttpPost("first-admin")]
        public async Task<IActionResult> CreateFirstAdmin([FromBody] UserRegistrationRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Admin data is required" });
            }

            try
            {
                // Check if any users already exist
                var existingUsers = await _userRepo.GetUsersAsync();
                
                if (existingUsers.Count > 0)
                {
                    return Conflict(new { 
                        message = "Users already exist in the system", 
                        usersCount = existingUsers.Count 
                    });
                }

                // Create the admin user
                var newUser = new User
                {
                    FirstName = request.FirstName ?? "Admin",
                    LastName = request.LastName ?? "User",
                    Email = request.Email,
                    PasswordHash = Argon2PasswordHasher.HashPassword(request.Password),
                    Role = "Admin",
                    Description = request.Description ?? "Initial admin user",
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };

                await _userRepo.CreateUserAsync(newUser);

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
                return StatusCode(500, new { 
                    error = ex.Message,
                    innerError = ex.InnerException?.Message 
                });
            }
        }
    }
}
