using innovaite_projects_dashboard.Models;

namespace innovaite_projects_dashboard.Persistence;

public interface IProjectDataAccess
{
    Task<List<Project>> GetProjectsAsync();
    Task<Project?> GetProjectAsync(string id);
    Task<List<Project>> GetProjectsByUserIdAsync(string userId);
    Task CreateProjectAsync(Project project);
    Task UpdateProjectAsync(string id, Project project);
    Task RemoveProjectAsync(string id);
}
