using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using innovaite_projects_dashboard.Models;
using innovaite_projects_dashboard.Persistence;

namespace innovaite_projects_dashboard.Controllers;

/// <summary>
/// Controller for managing projects in the InnovAIte Projects Dashboard system.
/// </summary>
[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectDataAccess _projectRepo;
    private readonly IUserDataAccess _userRepo;

    public ProjectsController(IProjectDataAccess projectRepo, IUserDataAccess userRepo)
    {
        _projectRepo = projectRepo;
        _userRepo = userRepo;
    }

    /// <summary>
    /// Get all projects.
    /// </summary>
    /// <returns>A list of all projects</returns>
    /// <response code="200">Returns the list of projects</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet()]
    public async Task<IActionResult> GetAllProjects()
    {
        var projects = await _projectRepo.GetProjectsAsync();
        return Ok(projects);
    }

    /// <summary>
    /// Get a specific project by ID.
    /// </summary>
    /// <param name="id">The ID of the project to retrieve</param>
    /// <returns>A project</returns>
    /// <response code="200">Returns the project</response>
    /// <response code="404">If the project is not found</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id}", Name = "GetProject")]
    public async Task<IActionResult> GetProjectById(string id)
    {
        var project = await _projectRepo.GetProjectAsync(id);
        if (project == null)
        {
            return NotFound();
        }
        return Ok(project);
    }

    /// <summary>
    /// Get all projects by a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>A list of projects by the user</returns>
    /// <response code="200">Returns the list of projects</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetProjectsByUserId(string userId)
    {
        var projects = await _projectRepo.GetProjectsByUserIdAsync(userId);
        return Ok(projects);
    }

    /// <summary>
    /// Get all projects by the current authenticated user.
    /// </summary>
    /// <returns>A list of projects by the current user</returns>
    /// <response code="200">Returns the list of projects</response>
    /// <response code="401">If the user is not authenticated</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    [HttpGet("my-projects")]
    public async Task<IActionResult> GetMyProjects()
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var projects = await _projectRepo.GetProjectsByUserIdAsync(userId);
        return Ok(projects);
    }

    /// <summary>
    /// Add a new project.
    /// </summary>
    /// <param name="request">The project data to add</param>
    /// <returns>A newly created project</returns>
    /// <response code="201">Returns the newly created project</response>
    /// <response code="400">If the project data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    [HttpPost()]
    public async Task<IActionResult> AddProject([FromBody] CreateProjectRequest request)
    {
        if (request == null) { 
            return BadRequest("Project data is required"); 
        }
        
        // Validate required fields
        if (string.IsNullOrEmpty(request.Title))
        {
            return BadRequest("Title is required");
        }
        
        if (string.IsNullOrEmpty(request.Description))
        {
            return BadRequest("Description is required");
        }
        
        var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User not authenticated");
        }

        var user = await _userRepo.GetUserAsync(userId);
        
        if (user == null)
        {
            return Unauthorized("User not found");
        }

        // Create the project from the request
        var newProject = new Project
        {
            Title = request.Title,
            Description = request.Description,
            GitHubUrl = request.GitHubUrl,
            LiveSiteUrl = request.LiveSiteUrl,
            Status = request.Status ?? "Not Started",
            Tools = request.Tools,
            UserId = userId,
            UserName = $"{user.FirstName} {user.LastName}",
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };
        
        try
        {
            await _projectRepo.CreateProjectAsync(newProject);
            return CreatedAtRoute("GetProject", new { id = newProject.Id }, newProject);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error creating project: {ex.Message}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Update a project.
    /// </summary>
    /// <param name="id">The ID of the project to update</param>
    /// <param name="updatedProject">The updated project data</param>
    /// <returns>No content</returns>
    /// <response code="204">If the project was updated successfully</response>
    /// <response code="400">If the project data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user doesn't own the project</response>
    /// <response code="404">If the project is not found</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProject(string id, Project updatedProject)
    {
        if (updatedProject == null) { return BadRequest(); }
        
        var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var existingProject = await _projectRepo.GetProjectAsync(id);
        if (existingProject == null) { return NotFound(); }
        
        // Check if user owns the project or is an admin
        if (existingProject.UserId != userId && userRole != "Admin")
        {
            return Forbid();
        }
        
        updatedProject.Id = id;
        updatedProject.UserId = existingProject.UserId;
        updatedProject.UserName = existingProject.UserName;
        
        await _projectRepo.UpdateProjectAsync(id, updatedProject);
        
        return NoContent();
    }

    /// <summary>
    /// Delete a project.
    /// </summary>
    /// <param name="id">The ID of the project to delete</param>
    /// <returns>No content</returns>
    /// <response code="204">If the project was deleted successfully</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user doesn't own the project</response>
    /// <response code="404">If the project is not found</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(string id)
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var project = await _projectRepo.GetProjectAsync(id);
        if (project == null) { return NotFound(); }
        
        // Check if user owns the project or is an admin
        if (project.UserId != userId && userRole != "Admin")
        {
            return Forbid();
        }
        
        await _projectRepo.RemoveProjectAsync(id);
        return NoContent();
    }
}
