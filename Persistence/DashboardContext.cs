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
            try
            {
                // Try multiple sources for the connection string, prioritizing environment variables
                var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") 
                    ?? configuration.GetConnectionString("MongoDb") 
                    ?? configuration.GetConnectionString("DefaultConnection") 
                    ?? "mongodb://localhost:27017";

                var source = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") != null 
                    ? "Environment Variable" 
                    : configuration.GetConnectionString("MongoDb") != null 
                        ? "Configuration (MongoDb)" 
                        : "Fallback";
                
                Console.WriteLine($"[INFO] Using MongoDB connection (source: {source})");
                Console.WriteLine($"[DEBUG] Connection string (masked): {MaskConnectionString(connectionString)}");
                
                MongoClientSettings settings = MongoClientSettings.FromConnectionString(connectionString);
                
                // Add a timeout to avoid hanging
                settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
                settings.ConnectTimeout = TimeSpan.FromSeconds(10);
                
                var client = new MongoClient(settings);
                
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
                
                // Test the connection
                try {
                    // Quick ping to check connection
                    var ping = _database.RunCommand<dynamic>(new MongoDB.Bson.BsonDocument("ping", 1));
                    Console.WriteLine($"[INFO] MongoDB connection successful. Server response time: {ping?.ok ?? 0}");
                } catch (Exception pingEx) {
                    Console.WriteLine($"[WARN] Database ping test failed: {pingEx.Message}");
                    // Continue anyway - don't throw here
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] MongoDB connection failed: {ex.Message}");
                Console.WriteLine($"[ERROR] Exception type: {ex.GetType().Name}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                
                // Rethrow after logging - this will cause the app to fail fast if DB connection is required
                throw;
            }
        }

        public DashboardContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("users");
        public IMongoCollection<Project> Projects => _database.GetCollection<Project>("projects");
        public IMongoCollection<Comment> Comments => _database.GetCollection<Comment>("comments");
        
        // Method to get the database name for health checks
        public string GetDatabaseName()
        {
            return _database.DatabaseNamespace.DatabaseName;
        }
        
        // Helper method to mask sensitive information in connection strings for logging
        private string MaskConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return "[empty]";
                
            try
            {
                // Mask password in format: mongodb+srv://username:password@cluster
                if (connectionString.Contains("://") && connectionString.Contains("@"))
                {
                    var parts = connectionString.Split('@');
                    var credentialPart = parts[0];
                    
                    if (credentialPart.Contains(":"))
                    {
                        var credParts = credentialPart.Split(':');
                        var protocol = credParts[0];
                        var username = credParts.Length > 1 ? credParts[1] : "";
                        
                        // Replace with masked version
                        return $"{protocol}:{username}:***@{parts[1]}";
                    }
                }
                
                // If we can't parse it, just return a generic mask
                return connectionString.Substring(0, Math.Min(10, connectionString.Length)) + "...";
            }
            catch
            {
                return "[masked-error]";
            }
        }
    }
}
