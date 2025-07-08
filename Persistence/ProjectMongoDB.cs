using MongoDB.Driver;
using innovaite_projects_dashboard.Models;
using MongoDB.Bson;

namespace innovaite_projects_dashboard.Persistence;

public class ProjectMongoDB : IProjectDataAccess
{
    private readonly IMongoCollection<Project> _projects;

    public ProjectMongoDB(DashboardContext context)
    {
        _projects = context.Projects;
    }

    public async Task<List<Project>> GetProjectsAsync()
    {
        return await _projects.Find(project => true).ToListAsync();
    }

    public async Task<Project?> GetProjectAsync(string id)
    {
        return await _projects.Find(project => project.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Project>> GetProjectsByUserIdAsync(string userId)
    {
        return await _projects.Find(project => project.UserId == userId).ToListAsync();
    }

    public async Task CreateProjectAsync(Project project)
    {
        project.CreatedDate = DateTime.UtcNow;
        project.ModifiedDate = DateTime.UtcNow;
        await _projects.InsertOneAsync(project);
    }

    public async Task UpdateProjectAsync(string id, Project project)
    {
        project.ModifiedDate = DateTime.UtcNow;
        await _projects.ReplaceOneAsync(p => p.Id == id, project);
    }

    public async Task RemoveProjectAsync(string id)
    {
        await _projects.DeleteOneAsync(project => project.Id == id);
    }
}
