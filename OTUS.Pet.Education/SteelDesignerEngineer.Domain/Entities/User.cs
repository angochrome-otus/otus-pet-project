using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SteelDesignerEngineer.Domain.Entities;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [BsonElement("lastName")]
    public string LastName { get; set; } = string.Empty;

    [BsonElement("role")]
    public string Role { get; set; } = "Student"; // Student, Teacher, Admin

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("lastLogin")]
    public DateTime? LastLogin { get; set; }

    [BsonElement("enrolledCourses")]
    public List<string> EnrolledCourses { get; set; } = new();

    [BsonElement("completedCourses")]
    public List<string> CompletedCourses { get; set; } = new();
}
