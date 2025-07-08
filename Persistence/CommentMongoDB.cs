using MongoDB.Driver;
using innovaite_projects_dashboard.Models;
using MongoDB.Bson;

namespace innovaite_projects_dashboard.Persistence;

public class CommentMongoDB : ICommentDataAccess
{
    private readonly IMongoCollection<Comment> _comments;

    public CommentMongoDB(DashboardContext context)
    {
        _comments = context.Comments;
    }

    public async Task<List<Comment>> GetCommentsAsync()
    {
        return await _comments.Find(comment => true).ToListAsync();
    }

    public async Task<Comment?> GetCommentAsync(string id)
    {
        return await _comments.Find(comment => comment.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Comment>> GetCommentsByProjectIdAsync(string projectId)
    {
        return await _comments.Find(comment => comment.ProjectId == projectId).ToListAsync();
    }

    public async Task CreateCommentAsync(Comment comment)
    {
        comment.CreatedDate = DateTime.UtcNow;
        comment.ModifiedDate = DateTime.UtcNow;
        await _comments.InsertOneAsync(comment);
    }

    public async Task UpdateCommentAsync(string id, Comment comment)
    {
        comment.ModifiedDate = DateTime.UtcNow;
        await _comments.ReplaceOneAsync(c => c.Id == id, comment);
    }

    public async Task RemoveCommentAsync(string id)
    {
        await _comments.DeleteOneAsync(comment => comment.Id == id);
    }
}
