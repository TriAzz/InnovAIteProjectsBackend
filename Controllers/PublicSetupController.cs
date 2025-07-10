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
        public async Task<IActionResult> GetHealth()
        {
            var healthInfo = new Dictionary<string, object>
            {
                ["status"] = "online",
                ["timestamp"] = DateTime.UtcNow,
                ["message"] = "Public setup API is accessible"
            };
            
            try
            {
                // Test database connection without requiring authentication
                var users = await _userRepo.GetUsersAsync();
                healthInfo["databaseConnection"] = "connected";
                healthInfo["usersCount"] = users.Count;
                healthInfo["firstUserExists"] = users.Count > 0;
            }
            catch (Exception ex)
            {
                healthInfo["databaseConnection"] = "error";
                healthInfo["errorType"] = ex.GetType().Name;
                healthInfo["errorMessage"] = ex.Message;
                healthInfo["hasMongoDbEnvVar"] = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING"));
            }
            
            return Ok(healthInfo);
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
                Console.WriteLine($"[INFO] Attempting to create first admin: {request.Email}");
                
                // Check if MongoDB connection works first
                try
                {
                    // Check if any users already exist
                    var existingUsers = await _userRepo.GetUsersAsync();
                    
                    Console.WriteLine($"[INFO] Found {existingUsers.Count} existing users");
                    
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
                    
                    Console.WriteLine($"[INFO] Admin user created successfully with ID: {newUser.Id}");

                    return Created($"/api/users/{newUser.Id}", new 
                    { 
                        message = "Admin user created successfully",
                        userId = newUser.Id,
                        email = newUser.Email,
                        role = newUser.Role
                    });
                }
                catch (MongoDB.Driver.MongoException mongoEx)
                {
                    Console.WriteLine($"[ERROR] MongoDB error creating admin: {mongoEx.Message}");
                    Console.WriteLine($"[ERROR] MongoDB stack trace: {mongoEx.StackTrace}");
                    
                    return StatusCode(500, new { 
                        error = "Database connection error",
                        errorType = mongoEx.GetType().Name,
                        errorDetails = mongoEx.Message,
                        innerError = mongoEx.InnerException?.Message 
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Unexpected error creating admin: {ex.Message}");
                
                return StatusCode(500, new { 
                    error = ex.Message,
                    errorType = ex.GetType().Name,
                    innerError = ex.InnerException?.Message,
                    stack = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Diagnostics endpoint to check database connectivity
        /// </summary>
        [HttpGet("diagnostics")]
        public IActionResult GetDiagnostics()
        {
            var diagnostics = new Dictionary<string, object>
            {
                ["timestamp"] = DateTime.UtcNow,
                ["serverInfo"] = new {
                    osVersion = Environment.OSVersion.ToString(),
                    machineName = Environment.MachineName,
                    processorCount = Environment.ProcessorCount,
                    dotnetVersion = Environment.Version.ToString(),
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                },
                ["mongoSettings"] = new {
                    hasEnvVar = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING")),
                    envVarLength = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING")?.Length ?? 0
                }
            };
            
            try
            {
                // Get system-level TLS info
                diagnostics["tlsSettings"] = new {
                    defaultSslProtocols = System.Security.Authentication.SslProtocols.Default,
                    supportsTls12 = System.Security.Authentication.SslProtocols.Tls12.HasFlag(System.Security.Authentication.SslProtocols.Default),
                    supportsTls13 = System.Security.Authentication.SslProtocols.Tls13.HasFlag(System.Security.Authentication.SslProtocols.Default)
                };
                
                // Check if MongoDB driver is available
                diagnostics["mongoDriverVersion"] = typeof(MongoDB.Driver.MongoClient).Assembly.GetName().Version.ToString();
                
                return Ok(diagnostics);
            }
            catch (Exception ex)
            {
                diagnostics["error"] = ex.Message;
                return Ok(diagnostics);
            }
        }
    }
}
