namespace SteelDesignerEngineer.Contracts.DTOs.User;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class GetUserRequest
{
    public string UserId { get; set; } = string.Empty;
}

public class GetUserResponse
{
    public bool Success { get; set; }
    public UserDto? User { get; set; }
    public string? Message { get; set; }
}

public class UpdateUserRequest
{
    public string UserId { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
}

public class UpdateUserResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
