namespace SteelDesignerEngineer.Domain.DTOs;

// ==========================================
// Authentication DTOs (Session-based)
// ==========================================

public record LoginRequest(string Email, string Password);

public record LoginResponse(
    bool Success,
    string Message,
    UserDto? User
);

public record RegisterRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    string Role = "Student" // Default to Student, can be "Student", "Teacher", or "Admin"
);

public record RegisterResponse(
    bool Success,
    string Message,
    UserDto? User
);

public record UpdateProfileRequest(
    string FirstName,
    string LastName
);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
);

public record UserDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    int EnrolledCoursesCount,
    int CompletedCoursesCount
)
{
    public string AuthProvider { get; init; } = "local";
    public string? AvatarUrl { get; init; }
    public string? LastLoginIp { get; init; }
    
    /// <summary>
    /// Full name (FirstName + LastName)
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
    
    // Student-specific
    public List<string>? EnrolledCourses { get; init; }
    public List<string>? CompletedCourses { get; init; }
    public int? CurrentSemester { get; init; }
    public string? StudentIdNumber { get; init; }
    
    // Teacher-specific
    public List<string>? TeachingCourses { get; init; }
    public string? Specialization { get; init; }
    public string? AcademicTitle { get; init; }
    public string? Bio { get; init; }
    
    // Admin-specific
    public List<string>? Permissions { get; init; }
    public string? Notes { get; init; }
}

// ==========================================
// OAuth DTOs
// ==========================================

public record OAuthCallbackRequest(
    string Provider,
    string Code,
    string? State
);

public record OAuthLoginResponse(
    bool Success,
    string Message,
    UserDto? User,
    bool IsNewUser
);

public record OAuthUserInfo(
    string ProviderId,
    string Email,
    string FirstName,
    string LastName,
    string? AvatarUrl,
    string Provider
);
