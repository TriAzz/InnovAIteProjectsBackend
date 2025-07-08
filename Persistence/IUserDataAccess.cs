using innovaite_projects_dashboard.Models;

namespace innovaite_projects_dashboard.Persistence;

public interface IUserDataAccess
{
    Task<List<User>> GetUsersAsync();
    Task<User?> GetUserAsync(string id);
    Task<User?> GetUserByEmailAsync(string email);
    Task CreateUserAsync(User user);
    Task UpdateUserAsync(string id, User user);
    Task RemoveUserAsync(string id);
}
