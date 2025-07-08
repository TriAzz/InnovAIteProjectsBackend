using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using innovaite_projects_dashboard.Models;
using innovaite_projects_dashboard.Persistence;

namespace innovaite_projects_dashboard.Controllers;

/// <summary>
/// Controller for managing comments in the InnovAIte Projects Dashboard system.
/// </summary>
[ApiController]
[Route("api/comments")]
public class CommentsController : ControllerBase
{
    private readonly ICommentDataAccess _commentRepo;
    private readonly IUserDataAccess _userRepo;
    private readonly IProjectDataAccess _projectRepo;

    public CommentsController(ICommentDataAccess commentRepo, IUserDataAccess userRepo, IProjectDataAccess projectRepo)
    {
        _commentRepo = commentRepo;
        _userRepo = userRepo;
        _projectRepo = projectRepo;
    }

    /// <summary>
    /// Get all comments.
    /// </summary>
    /// <returns>A list of all comments</returns>
    /// <response code="200">Returns the list of comments</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet()]
    public async Task<IActionResult> GetAllComments()
    {
        var comments = await _commentRepo.GetCommentsAsync();
        return Ok(comments);
    }

    /// <summary>
    /// Get a specific comment by ID.
    /// </summary>
    /// <param name="id">The ID of the comment to retrieve</param>
    /// <returns>A comment</returns>
    /// <response code="200">Returns the comment</response>
    /// <response code="404">If the comment is not found</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id}", Name = "GetComment")]
    public async Task<IActionResult> GetCommentById(string id)
    {
        var comment = await _commentRepo.GetCommentAsync(id);
        if (comment == null)
        {
            return NotFound();
        }
        return Ok(comment);
    }

    /// <summary>
    /// Get all comments for a specific project.
    /// </summary>
    /// <param name="projectId">The ID of the project</param>
    /// <returns>A list of comments for the project</returns>
    /// <response code="200">Returns the list of comments</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetCommentsByProjectId(string projectId)
    {
        var comments = await _commentRepo.GetCommentsByProjectIdAsync(projectId);
        return Ok(comments);
    }

    /// <summary>
    /// Get all approved comments for a specific project.
    /// </summary>
    /// <param name="projectId">The ID of the project</param>
    /// <returns>A list of approved comments for the project</returns>
    /// <response code="200">Returns the list of approved comments</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("project/{projectId}/approved")]
    public async Task<IActionResult> GetApprovedCommentsByProjectId(string projectId)
    {
        var comments = await _commentRepo.GetCommentsByProjectIdAsync(projectId);
        var approvedComments = comments.Where(c => c.Approved).ToList();
        return Ok(approvedComments);
    }

    /// <summary>
    /// Add a new comment.
    /// </summary>
    /// <param name="request">The comment data to add</param>
    /// <returns>A newly created comment</returns>
    /// <response code="201">Returns the newly created comment</response>
    /// <response code="400">If the comment data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="404">If the project is not found</response>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    [HttpPost()]
    public async Task<IActionResult> AddComment([FromBody] CreateCommentRequest request)
    {
        if (request == null) { 
            return BadRequest("Comment data is required"); 
        }
        
        if (string.IsNullOrEmpty(request.Content))
        {
            return BadRequest("Content is required");
        }
        
        if (string.IsNullOrEmpty(request.ProjectId))
        {
            return BadRequest("ProjectId is required");
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

        var project = await _projectRepo.GetProjectAsync(request.ProjectId);
        if (project == null)
        {
            return NotFound("Project not found");
        }

        // Create the comment from the request
        var newComment = new Comment
        {
            ProjectId = request.ProjectId,
            Content = request.Content,
            UserId = userId,
            UserName = $"{user.FirstName} {user.LastName}",
            Approved = true, // Comments are automatically approved and visible
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };
        
        try
        {
            await _commentRepo.CreateCommentAsync(newComment);
            return CreatedAtRoute("GetComment", new { id = newComment.Id }, newComment);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error creating comment: {ex.Message}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Update a comment.
    /// </summary>
    /// <param name="id">The ID of the comment to update</param>
    /// <param name="updatedComment">The updated comment data</param>
    /// <returns>No content</returns>
    /// <response code="204">If the comment was updated successfully</response>
    /// <response code="400">If the comment data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user doesn't own the comment</response>
    /// <response code="404">If the comment is not found</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateComment(string id, Comment updatedComment)
    {
        if (updatedComment == null) { return BadRequest(); }
        
        var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var existingComment = await _commentRepo.GetCommentAsync(id);
        if (existingComment == null) { return NotFound(); }
        
        // Check if user owns the comment or is an admin
        if (existingComment.UserId != userId && userRole != "Admin")
        {
            return Forbid();
        }
        
        updatedComment.Id = id;
        updatedComment.UserId = existingComment.UserId;
        updatedComment.UserName = existingComment.UserName;
        updatedComment.ProjectId = existingComment.ProjectId;
        
        await _commentRepo.UpdateCommentAsync(id, updatedComment);
        
        return NoContent();
    }

    /// <summary>
    /// Delete a comment.
    /// </summary>
    /// <param name="id">The ID of the comment to delete</param>
    /// <returns>No content</returns>
    /// <response code="204">If the comment was deleted successfully</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user doesn't own the comment</response>
    /// <response code="404">If the comment is not found</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(string id)
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var comment = await _commentRepo.GetCommentAsync(id);
        if (comment == null) { return NotFound(); }
        
        // Check if user owns the comment or is an admin
        if (comment.UserId != userId && userRole != "Admin")
        {
            return Forbid();
        }
        
        await _commentRepo.RemoveCommentAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Approve a comment (Admin only).
    /// </summary>
    /// <param name="id">The ID of the comment to approve</param>
    /// <returns>No content</returns>
    /// <response code="204">If the comment was approved successfully</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    /// <response code="404">If the comment is not found</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Policy = "AdminOnly")]
    [HttpPatch("{id}/approve")]
    public async Task<IActionResult> ApproveComment(string id)
    {
        var comment = await _commentRepo.GetCommentAsync(id);
        if (comment == null) { return NotFound(); }
        
        comment.Approved = true;
        await _commentRepo.UpdateCommentAsync(id, comment);
        
        return NoContent();
    }

    /// <summary>
    /// Unapprove a comment (Admin only).
    /// </summary>
    /// <param name="id">The ID of the comment to unapprove</param>
    /// <returns>No content</returns>
    /// <response code="204">If the comment was unapproved successfully</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not an admin</response>
    /// <response code="404">If the comment is not found</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Policy = "AdminOnly")]
    [HttpPatch("{id}/unapprove")]
    public async Task<IActionResult> UnapproveComment(string id)
    {
        var comment = await _commentRepo.GetCommentAsync(id);
        if (comment == null) { return NotFound(); }
        
        comment.Approved = false;
        await _commentRepo.UpdateCommentAsync(id, comment);
        
        return NoContent();
    }
}
