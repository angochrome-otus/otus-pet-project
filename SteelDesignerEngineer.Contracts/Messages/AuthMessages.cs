namespace SteelDesignerEngineer.Contracts.Messages;

// ==========================================
// Request-Reply Messages for Auth Service
// ==========================================

/// <summary>
/// Base message with correlation ID for request-reply pattern
/// </summary>
public abstract record BaseMessage
{
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Register new user request
/// </summary>
public record RegisterUserRequest : BaseMessage
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Role { get; init; }
    public string? IpAddress { get; init; }
}

/// <summary>
/// Register user response
/// </summary>
public record RegisterUserResponse : BaseMessage
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public string? UserId { get; init; }
    public string? Email { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Role { get; init; }
}

/// <summary>
/// Login request
/// </summary>
public record LoginUserRequest : BaseMessage
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public string? IpAddress { get; init; }
}

/// <summary>
/// Login response
/// </summary>
public record LoginUserResponse : BaseMessage
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public string? UserId { get; init; }
    public string? Email { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Role { get; init; }
    public string? AvatarUrl { get; init; }
}

/// <summary>
/// Get user by ID request
/// </summary>
public record GetUserRequest : BaseMessage
{
    public required string UserId { get; init; }
}

/// <summary>
/// Get user response
/// </summary>
public record GetUserResponse : BaseMessage
{
    public bool Success { get; init; }
    public string? UserId { get; init; }
    public string? Email { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Role { get; init; }
    public string? AvatarUrl { get; init; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
}

/// <summary>
/// Update user profile request
/// </summary>
public record UpdateProfileRequest : BaseMessage
{
    public required string UserId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
}

/// <summary>
/// Update profile response
/// </summary>
public record UpdateProfileResponse : BaseMessage
{
    public bool Success { get; init; }
    public string? Message { get; init; }
}

/// <summary>
/// Change password request
/// </summary>
public record ChangePasswordRequest : BaseMessage
{
    public required string UserId { get; init; }
    public required string CurrentPassword { get; init; }
    public required string NewPassword { get; init; }
}

/// <summary>
/// Change password response
/// </summary>
public record ChangePasswordResponse : BaseMessage
{
    public bool Success { get; init; }
    public string? Message { get; init; }
}

/// <summary>
/// OAuth login request
/// </summary>
public record OAuthLoginRequest : BaseMessage
{
    public required string ProviderId { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Provider { get; init; }
    public string? AvatarUrl { get; init; }
    public string? IpAddress { get; init; }
}

/// <summary>
/// OAuth login response
/// </summary>
public record OAuthLoginResponse : BaseMessage
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public string? UserId { get; init; }
    public bool IsNewUser { get; init; }
}
