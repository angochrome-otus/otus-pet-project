namespace SteelDesignerEngineer.Domain.Services;

/// <summary>
/// Сервис для хеширования и проверки паролей
/// Clean Architecture: Domain Layer - интерфейс
/// </summary>
public interface IPasswordHashService
{
    /// <summary>
    /// Хеширует пароль
    /// </summary>
    string HashPassword(string password);
    
    /// <summary>
    /// Проверяет соответствие пароля хешу
    /// </summary>
    bool VerifyPassword(string password, string passwordHash);
}
