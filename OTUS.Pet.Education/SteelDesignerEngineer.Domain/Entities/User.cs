using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SteelDesignerEngineer.Domain.Entities;

/// <summary>
/// Base User entity - Domain Model
/// Clean Architecture: Domain Layer
/// Abstract base class for Student, Teacher, and Admin
/// </summary>
[BsonDiscriminator(RootClass = true)]
[BsonKnownTypes(typeof(Student), typeof(Teacher), typeof(Admin))]
public abstract class User
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// BCrypt password hash (nullable for OAuth users)
    /// </summary>
    public string? PasswordHash { get; set; }
    
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// User role: "Student", "Teacher", "Admin"
    /// Implemented by derived classes
    /// </summary>
    public abstract string Role { get; }
    
    /// <summary>
    /// OAuth provider: "local", "google", "github"
    /// </summary>
    public string AuthProvider { get; set; } = "local";
    
    /// <summary>
    /// OAuth provider user ID (Google sub, GitHub id)
    /// </summary>
    public string? OAuthProviderId { get; set; }
    
    /// <summary>
    /// Avatar URL from OAuth provider or custom upload
    /// </summary>
    public string? AvatarUrl { get; set; }
    
    /// <summary>
    /// Is account active?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Account creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
    
    /// <summary>
    /// IP address of last login
    /// </summary>
    public string? LastLoginIp { get; set; }
    
    /// <summary>
    /// Get full name
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
}

/// <summary>
/// Student entity - inherits from User
/// </summary>
[BsonDiscriminator("Student")]
public class Student : User
{
    public override string Role => "Student";
    
    /// <summary>
    /// Courses the student is enrolled in
    /// </summary>
    public List<string> EnrolledCourses { get; set; } = new();
    
    /// <summary>
    /// Courses the student has completed
    /// </summary>
    public List<string> CompletedCourses { get; set; } = new();
    
    /// <summary>
    /// Student's current semester/year
    /// </summary>
    public int? CurrentSemester { get; set; }
    
    /// <summary>
    /// Student ID number
    /// </summary>
    public string? StudentIdNumber { get; set; }
}

/// <summary>
/// Teacher entity - inherits from User
/// </summary>
[BsonDiscriminator("Teacher")]
public class Teacher : User
{
    public override string Role => "Teacher";
    
    /// <summary>
    /// Courses the teacher is teaching
    /// </summary>
    public List<string> TeachingCourses { get; set; } = new();
    
    /// <summary>
    /// Teacher's specialization/department
    /// </summary>
    public string? Specialization { get; set; }
    
    /// <summary>
    /// Academic title (Professor, Associate Professor, etc.)
    /// </summary>
    public string? AcademicTitle { get; set; }
    
    /// <summary>
    /// Teacher's bio/description
    /// </summary>
    public string? Bio { get; set; }
}

/// <summary>
/// Admin entity - inherits from User
/// </summary>
[BsonDiscriminator("Admin")]
public class Admin : User
{
    public override string Role => "Admin";
    
    /// <summary>
    /// Admin's permissions/access level
    /// </summary>
    public List<string> Permissions { get; set; } = new()
    {
        "ManageUsers",
        "ManageCourses",
        "ViewReports",
        "SystemSettings"
    };
    
    /// <summary>
    /// Admin notes/comments
    /// </summary>
    public string? Notes { get; set; }
}
