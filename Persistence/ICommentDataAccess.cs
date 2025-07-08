using innovaite_projects_dashboard.Models;

namespace innovaite_projects_dashboard.Persistence;

public interface ICommentDataAccess
{
    Task<List<Comment>> GetCommentsAsync();
    Task<Comment?> GetCommentAsync(string id);
    Task<List<Comment>> GetCommentsByProjectIdAsync(string projectId);
    Task CreateCommentAsync(Comment comment);
    Task UpdateCommentAsync(string id, Comment comment);
    Task RemoveCommentAsync(string id);
}
