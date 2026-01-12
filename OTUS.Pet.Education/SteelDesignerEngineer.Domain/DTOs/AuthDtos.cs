namespace SteelDesignerEngineer.Domain.DTOs;

// Login
public record LoginRequest(
    string Email,
    string Password,
    bool RememberMe = false
);

public record LoginResponse(
    bool Success,
    string? Token,
    string? RefreshToken,
    string? Message,
    UserDto? User
);

// Register
public record RegisterRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName
);

public record RegisterResponse(
    bool Success,
    string? Token,
    string? Message,
    UserDto? User
);

// User DTO
public record UserDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    DateTime CreatedAt,
    DateTime? LastLogin,
    int EnrolledCoursesCount,
    int CompletedCoursesCount
);

// Update Profile
public record UpdateProfileRequest(
    string FirstName,
    string LastName
);

// Change Password
public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
);
