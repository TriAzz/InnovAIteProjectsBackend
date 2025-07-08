using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace innovaite_projects_dashboard.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("email")]
    public string Email { get; set; } = null!;

    [BsonElement("firstName")]
    public string FirstName { get; set; } = null!;

    [BsonElement("lastName")]
    public string LastName { get; set; } = null!;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = null!;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("role")]
    public string? Role { get; set; }

    [BsonElement("createdDate")]
    public DateTime CreatedDate { get; set; }

    [BsonElement("modifiedDate")]
    public DateTime ModifiedDate { get; set; }

    // Default constructor
    public User()
    {
    }
}
