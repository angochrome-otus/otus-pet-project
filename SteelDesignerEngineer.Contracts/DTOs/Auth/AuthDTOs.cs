namespace SteelDesignerEngineer.Contracts.DTOs.Auth;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public string? Message { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class RegisterResponse
{
    public bool Success { get; set; }
    public string? UserId { get; set; }
    public string? Message { get; set; }
}

public class LogoutRequest
{
    public string UserId { get; set; } = string.Empty;
    public string? SessionId { get; set; }
}
