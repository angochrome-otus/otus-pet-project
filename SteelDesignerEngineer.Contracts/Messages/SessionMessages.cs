namespace SteelDesignerEngineer.Contracts.Messages;

// ==========================================
// Request-Reply Messages for Session Service
// ==========================================

/// <summary>
/// Create session request
/// </summary>
public record CreateSessionRequest : BaseMessage
{
    public required string UserId { get; init; }
    public required string UserName { get; init; }
    public required string UserEmail { get; init; }
    public required string UserRole { get; init; }
    public int ExpirationMinutes { get; init; } = 1440; // 24 hours
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}

/// <summary>
/// Create session response
/// </summary>
public record CreateSessionResponse : BaseMessage
{
    public bool Success { get; init; }
    public string? SessionId { get; init; }
    public DateTime? ExpiresAt { get; init; }
}

/// <summary>
/// Validate session request
/// </summary>
public record ValidateSessionRequest : BaseMessage
{
    public required string SessionId { get; init; }
}

/// <summary>
/// Validate session response
/// </summary>
public record ValidateSessionResponse : BaseMessage
{
    public bool IsValid { get; init; }
    public string? Message { get; init; }
    public string? UserId { get; init; }
    public string? UserName { get; init; }
    public string? UserEmail { get; init; }
    public string? UserRole { get; init; }
}

/// <summary>
/// Delete session request (logout)
/// </summary>
public record DeleteSessionRequest : BaseMessage
{
    public required string SessionId { get; init; }
}

/// <summary>
/// Delete session response
/// </summary>
public record DeleteSessionResponse : BaseMessage
{
    public bool Success { get; init; }
    public string? Message { get; init; }
}

/// <summary>
/// Extend session request
/// </summary>
public record ExtendSessionRequest : BaseMessage
{
    public required string SessionId { get; init; }
    public int ExpirationMinutes { get; init; } = 1440; // 24 hours
}

/// <summary>
/// Extend session response
/// </summary>
public record ExtendSessionResponse : BaseMessage
{
    public bool Success { get; init; }
    public DateTime? ExpiresAt { get; init; }
}

/// <summary>
/// Get user sessions request
/// </summary>
public record GetUserSessionsRequest : BaseMessage
{
    public required string UserId { get; init; }
    public bool ActiveOnly { get; init; } = true;
}

/// <summary>
/// Get user sessions response
/// </summary>
public record GetUserSessionsResponse : BaseMessage
{
    public bool Success { get; init; }
    public List<SessionInfo> Sessions { get; init; } = new();
}

/// <summary>
/// Session information
/// </summary>
public record SessionInfo
{
    public string SessionId { get; init; } = string.Empty;
    public DateTime LoginAt { get; init; }
    public DateTime? LogoutAt { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public bool IsActive { get; init; }
}
