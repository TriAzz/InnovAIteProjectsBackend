using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using innovaite_projects_dashboard.Models;
using innovaite_projects_dashboard.Persistence;
using innovaite_projects_dashboard.Authentication;

namespace innovaite_projects_dashboard.Controllers;

/// <summary>
/// Controller for managing users in the InnovAIte Projects Dashboard system.
/// </summary>
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserDataAccess _userRepo;

    public UsersController(IUserDataAccess userRepo)
    {
        _userRepo = userRepo;
    }

    /// <summary>
    /// Get all users.
    /// </summary>
    /// <returns>A list of all users</returns>
    /// <response code="200">Returns the list of users</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet()]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userRepo.GetUsersAsync();
        return Ok(users);
    }

    /// <summary>
    /// Get a specific user by ID.
    /// </summary>
    /// <param name="id">The ID of the user to retrieve</param>
    /// <returns>A user</returns>
    /// <response code="200">Returns the user</response>
    /// <response code="404">If the user is not found</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id}", Name = "GetUser")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _userRepo.GetUserAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    /// <summary>
    /// Get all admin users.
    /// </summary>
    /// <returns>A list of admin users</returns>
    /// <response code="200">Returns the list of admin users</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("admins")]
    public async Task<IActionResult> GetAdminUsers()
    {
        var users = await _userRepo.GetUsersAsync();
        var adminUsers = users.Where(u => u.Role == "Admin").ToList();
        return Ok(adminUsers);
    }
    
    /// <summary>
    /// Get the currently authenticated user.
    /// </summary>
    /// <returns>The current user</returns>
    /// <response code="200">Returns the current user</response>
    /// <response code="401">If the user is not authenticated</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _userRepo.GetUserAsync(userId);
        if (user == null)
        {
            return Unauthorized();
        }        
        return Ok(user);
    }

    /// <summary>
    /// Add a new user.
    /// </summary>
    /// <param name="newUser">The user to add</param>
    /// <returns>A newly created user</returns>
    /// <response code="201">Returns the newly created user</response>
    /// <response code="400">If the user data is invalid</response>
    /// <response code="409">If a user with the same email already exists</response>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPost()]
    public async Task<IActionResult> AddUser(User newUser)
    {
        if (newUser == null) { return BadRequest(); }
        
        var existingUser = await _userRepo.GetUserByEmailAsync(newUser.Email);
        if (existingUser != null)
        {
            return Conflict(existingUser);
        }
        
        // Hash the password
        newUser.PasswordHash = Argon2PasswordHasher.HashPassword(newUser.PasswordHash);
        
        await _userRepo.CreateUserAsync(newUser);
        return CreatedAtRoute("GetUser", new { id = newUser.Id }, newUser);
    }

    /// <summary>
    /// Update a user.
    /// </summary>
    /// <param name="id">The ID of the user to update</param>
    /// <param name="updatedUser">The updated user data</param>
    /// <returns>No content</returns>
    /// <response code="204">If the user was updated successfully</response>
    /// <response code="400">If the user data is invalid</response>
    /// <response code="404">If the user is not found</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, User updatedUser)
    {
        if (updatedUser == null) { return BadRequest(); }
        
        var existingUser = await _userRepo.GetUserAsync(id);
        if (existingUser == null) { return NotFound(); }
        
        updatedUser.Id = id;
        await _userRepo.UpdateUserAsync(id, updatedUser);
        
        return NoContent();
    }

    /// <summary>
    /// Delete a user.
    /// </summary>
    /// <param name="id">The ID of the user to delete</param>
    /// <returns>No content</returns>
    /// <response code="204">If the user was deleted successfully</response>
    /// <response code="404">If the user is not found</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userRepo.GetUserAsync(id);
        if (user == null) { return NotFound(); }
        
        await _userRepo.RemoveUserAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Update user security information (email and password).
    /// </summary>
    /// <param name="id">The ID of the user to update</param>
    /// <param name="loginModel">The login credentials to update</param>
    /// <returns>No content</returns>
    /// <response code="204">If the user security was updated successfully</response>
    /// <response code="400">If the login data is invalid</response>
    /// <response code="404">If the user is not found</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPatch("{id}/security")]
    public async Task<IActionResult> UpdateUserSecurity(string id, LoginModel loginModel)
    {
        if (loginModel == null) { return BadRequest(); }
        
        var user = await _userRepo.GetUserAsync(id);
        if (user == null) { return NotFound(); }
        
        user.Email = loginModel.Email;
        user.PasswordHash = Argon2PasswordHasher.HashPassword(loginModel.Password);
        
        await _userRepo.UpdateUserAsync(id, user);
        
        return NoContent();
    }

    /// <summary>
    /// Create the first admin user (for initial setup only).
    /// </summary>
    /// <param name="request">The admin user registration information</param>
    /// <returns>A newly created admin user</returns>
    /// <response code="201">Returns the newly created admin user</response>
    /// <response code="400">If the user data is invalid</response>
    /// <response code="409">If users already exist in the system</response>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPost("setup-admin")]
    public async Task<IActionResult> SetupAdminUser(UserRegistrationRequest request)
    {
        if (request == null) { return BadRequest("User data is required"); }
        
        // Check if any users already exist
        var existingUsers = await _userRepo.GetUsersAsync();
        if (existingUsers.Count > 0) 
        { 
            return Conflict("Admin user already exists. Use regular admin endpoints."); 
        }
        
        // Create user object with hashed password
        var newUser = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = Argon2PasswordHasher.HashPassword(request.Password),
            Role = "Admin", // Force admin role for first user
            Description = request.Description
        };
        
        await _userRepo.CreateUserAsync(newUser);
        return CreatedAtRoute("GetUser", new { id = newUser.Id }, newUser);
    }
}
