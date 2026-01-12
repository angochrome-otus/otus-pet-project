using SteelDesignerEngineer.Domain.DTOs;

namespace SteelDesignerEngineer.Application.Interfaces;

/// <summary>
/// Application Service для работы с авторизацией
/// Clean Architecture: Application Layer - Use Cases
/// </summary>
public interface IAuthApplicationService
{
    /// <summary>
    /// Авторизация пользователя
    /// </summary>
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получить пользователя по ID
    /// </summary>
    Task<UserDto?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Обновить профиль пользователя
    /// </summary>
    Task<UserDto?> UpdateProfileAsync(string userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Изменить пароль
    /// </summary>
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}
