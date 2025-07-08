using MongoDB.Driver;
using innovaite_projects_dashboard.Models;
using MongoDB.Bson;

namespace innovaite_projects_dashboard.Persistence;

public class UserMongoDB : IUserDataAccess
{
    private readonly IMongoCollection<User> _users;

    public UserMongoDB(DashboardContext context)
    {
        _users = context.Users;
    }

    public async Task<List<User>> GetUsersAsync()
    {
        return await _users.Find(user => true).ToListAsync();
    }

    public async Task<User?> GetUserAsync(string id)
    {
        return await _users.Find(user => user.Id == id).FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _users.Find(user => user.Email == email).FirstOrDefaultAsync();
    }

    public async Task CreateUserAsync(User user)
    {
        user.CreatedDate = DateTime.UtcNow;
        user.ModifiedDate = DateTime.UtcNow;
        await _users.InsertOneAsync(user);
    }

    public async Task UpdateUserAsync(string id, User user)
    {
        user.ModifiedDate = DateTime.UtcNow;
        await _users.ReplaceOneAsync(u => u.Id == id, user);
    }

    public async Task RemoveUserAsync(string id)
    {
        await _users.DeleteOneAsync(user => user.Id == id);
    }
}
