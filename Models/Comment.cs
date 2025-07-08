using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace innovaite_projects_dashboard.Models;

public class Comment
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("projectId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ProjectId { get; set; } = null!;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = null!;

    [BsonElement("userName")]
    public string UserName { get; set; } = null!;

    [BsonElement("content")]
    public string Content { get; set; } = null!;

    [BsonElement("approved")]
    public bool Approved { get; set; } = false;

    [BsonElement("createdDate")]
    public DateTime CreatedDate { get; set; }

    [BsonElement("modifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
