using MongoDB.Driver;
using innovaite_projects_dashboard.Models;
using Microsoft.Extensions.Configuration;

namespace innovaite_projects_dashboard.Persistence
{
    public class DashboardContext
    {
        private readonly IMongoDatabase _database;

        public DashboardContext(IConfiguration configuration)
        {
            var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") 
                ?? configuration.GetConnectionString("DefaultConnection") 
                ?? "mongodb://localhost:27017";
            
            Console.WriteLine($"[INFO] Using MongoDB connection (source: {(Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") != null ? "Environment Variable" : "Configuration")})");
            var client = new MongoClient(connectionString);
            
            // Extract database name from connection string or use default
            var databaseName = "innovaite_projects_dashboard";
            if (connectionString.Contains("/") && !connectionString.EndsWith("/"))
            {
                var parts = connectionString.Split('/');
                if (parts.Length > 3)
                {
                    var dbPart = parts[^1]; // Get last part
                    // Remove query parameters if they exist
                    if (dbPart.Contains("?"))
                    {
                        databaseName = dbPart.Split('?')[0];
                    }
                    else
                    {
                        databaseName = dbPart;
                    }
                }
            }
            
            Console.WriteLine($"[DEBUG] Database name extracted: {databaseName}");
            _database = client.GetDatabase(databaseName);
        }

        public DashboardContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("users");
        public IMongoCollection<Project> Projects => _database.GetCollection<Project>("projects");
        public IMongoCollection<Comment> Comments => _database.GetCollection<Comment>("comments");
    }
}
