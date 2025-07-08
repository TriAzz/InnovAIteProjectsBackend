using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace innovaite_projects_dashboard.Models;

public class Project
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = null!;

    [BsonElement("description")]
    public string Description { get; set; } = null!;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = null!;

    [BsonElement("userName")]
    public string UserName { get; set; } = null!;

    [BsonElement("githubUrl")]
    public string? GitHubUrl { get; set; }

    [BsonElement("liveSiteUrl")]
    public string? LiveSiteUrl { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = "Not Started"; // Not Started, In Progress, Completed

    [BsonElement("tools")]
    public string? Tools { get; set; }

    [BsonElement("createdDate")]
    public DateTime CreatedDate { get; set; }

    [BsonElement("modifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
