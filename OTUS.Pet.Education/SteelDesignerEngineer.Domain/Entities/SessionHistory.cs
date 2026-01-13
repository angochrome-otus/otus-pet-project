using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SteelDesignerEngineer.Domain.Entities;

/// <summary>
/// Session history entity - отслеживание входов/выходов пользователей
/// Clean Architecture: Domain Layer
/// MongoDB: Source of Truth для истории сессий
/// </summary>
public class SessionHistory
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// User ID
    /// </summary>
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// User's full name (FirstName LastName)
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// User's email
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// User's role (Student, Teacher, Admin)
    /// </summary>
    public string UserRole { get; set; } = string.Empty;

    /// <summary>
    /// Session ID (хранится в Redis)
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Время начала сессии (login)
    /// </summary>
    public DateTime LoginAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Время завершения сессии (logout или expiration)
    /// </summary>
    public DateTime? LogoutAt { get; set; }

    /// <summary>
    /// IP адрес пользователя
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User Agent (браузер)
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Тип завершения: "manual" (явный logout), "expired" (истекла), "revoked" (принудительно)
    /// </summary>
    public string? LogoutType { get; set; }

    /// <summary>
    /// Длительность сессии в секундах
    /// </summary>
    public long? DurationSeconds { get; set; }

    /// <summary>
    /// Активна ли сессия
    /// </summary>
    public bool IsActive { get; set; } = true;
}
