using SteelDesignerEngineer.Domain.Entities;
using System.Security.Claims;

namespace SteelDesignerEngineer.Domain.Services;

/// <summary>
/// Сервис для генерации и валидации JWT токенов
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Генерирует JWT токен для пользователя
    /// </summary>
    string GenerateAccessToken(User user);
    
    /// <summary>
    /// Генерирует Refresh токен
    /// </summary>
    string GenerateRefreshToken();
    
    /// <summary>
    /// Получает Claims из токена
    /// </summary>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    
    /// <summary>
    /// Валидирует токен
    /// </summary>
    bool ValidateToken(string token);
}
