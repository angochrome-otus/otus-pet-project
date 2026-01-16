using SteelDesignerEngineer.Domain.Services;

namespace SteelDesignerEngineer.Infrastructure.Services;

/// <summary>
/// Реализация сервиса хеширования паролей с использованием BCrypt
/// Clean Architecture: Infrastructure Layer
/// </summary>
public class PasswordHashService : IPasswordHashService
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }
}
