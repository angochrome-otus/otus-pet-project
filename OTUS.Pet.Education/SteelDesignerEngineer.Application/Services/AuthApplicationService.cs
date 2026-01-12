using Microsoft.Extensions.Logging;
using SteelDesignerEngineer.Application.Interfaces;
using SteelDesignerEngineer.Domain.DTOs;
using SteelDesignerEngineer.Domain.Entities;
using SteelDesignerEngineer.Domain.Repositories;
using SteelDesignerEngineer.Domain.Services;

namespace SteelDesignerEngineer.Application.Services;

/// <summary>
/// Application Service для авторизации - координирует Domain Services
/// Clean Architecture: Application Layer
/// MongoDB - источник истины, Redis - кеш для производительности
/// </summary>
public class AuthApplicationService : IAuthApplicationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IAuthCacheService _authCacheService;
    private readonly ILogger<AuthApplicationService> _logger;

    private const int MaxLoginAttempts = 5; // Максимум попыток входа за 15 минут

    public AuthApplicationService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHashService passwordHashService,
        IAuthCacheService authCacheService,
        ILogger<AuthApplicationService> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHashService = passwordHashService;
        _authCacheService = authCacheService;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // 0. Rate limiting - проверка попыток входа в Redis
            var attempts = await _authCacheService.GetLoginAttemptsCountAsync(request.Email, cancellationToken);
            if (attempts >= MaxLoginAttempts)
            {
                _logger.LogWarning("Too many login attempts for {Email}: {Attempts}", request.Email, attempts);
                return new LoginResponse(false, null, null, $"Слишком много попыток входа. Попробуйте через 15 минут.", null);
            }

            // 1. Найти пользователя в MongoDB (источник истины!)
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null || !user.IsActive)
            {
                await _authCacheService.RecordLoginAttemptAsync(request.Email, cancellationToken);
                _logger.LogWarning("Login attempt failed: User not found or inactive - {Email}", request.Email);
                return new LoginResponse(false, null, null, "Неверный email или пароль", null);
            }

            // 2. Проверить пароль (хеш из MongoDB)
            if (!_passwordHashService.VerifyPassword(request.Password, user.PasswordHash))
            {
                await _authCacheService.RecordLoginAttemptAsync(request.Email, cancellationToken);
                _logger.LogWarning("Login attempt failed: Invalid password - {Email}", request.Email);
                return new LoginResponse(false, null, null, "Неверный email или пароль", null);
            }

            // 3. Успешный вход - сбросить счетчик попыток в Redis
            await _authCacheService.ResetLoginAttemptsAsync(request.Email, cancellationToken);

            // 4. Обновить lastLogin в MongoDB
            user.LastLogin = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user, cancellationToken);

            // 5. Генерировать JWT токен (НЕ сохраняется в БД)
            var token = _jwtTokenService.GenerateAccessToken(user);
            
            // 6. Генерировать и СОХРАНИТЬ Refresh токен в MongoDB
            var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();
            var refreshToken = new RefreshToken
            {
                Token = refreshTokenValue,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(30), // 30 дней
                CreatedAt = DateTime.UtcNow
            };
            await _refreshTokenRepository.CreateAsync(refreshToken, cancellationToken);

            // 7. Кешировать Refresh Token в Redis для быстрой проверки
            await _authCacheService.CacheRefreshTokenAsync(
                refreshTokenValue, 
                user.Id, 
                TimeSpan.FromDays(30), 
                cancellationToken);

            // 8. Кешировать User в Redis для быстрого доступа
            await _authCacheService.CacheUserAsync(user, cancellationToken);

            // 9. Вернуть результат
            var userDto = MapToDto(user);
            _logger.LogInformation("User logged in successfully: {Email}", request.Email);
            
            return new LoginResponse(true, token, refreshTokenValue, "Успешный вход", userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return new LoginResponse(false, null, null, "Ошибка авторизации", null);
        }
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Валидация
            if (request.Password != request.ConfirmPassword)
            {
                return new RegisterResponse(false, null, "Пароли не совпадают", null);
            }

            // 2. Проверить что email не занят в MongoDB
            var exists = await _userRepository.ExistsAsync(request.Email, cancellationToken);
            if (exists)
            {
                _logger.LogWarning("Registration attempt failed: Email already exists - {Email}", request.Email);
                return new RegisterResponse(false, null, "Email уже используется", null);
            }

            // 3. Создать пользователя и СОХРАНИТЬ в MongoDB (источник истины!)
            var user = new User
            {
                Email = request.Email,
                PasswordHash = _passwordHashService.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Role = "Student" // По умолчанию Student
            };

            user = await _userRepository.CreateAsync(user, cancellationToken);

            // 4. Кешировать User в Redis
            await _authCacheService.CacheUserAsync(user, cancellationToken);

            // 5. Генерировать JWT токен (НЕ сохраняется в БД)
            var token = _jwtTokenService.GenerateAccessToken(user);

            // 6. Вернуть результат
            var userDto = MapToDto(user);
            _logger.LogInformation("User registered successfully: {Email}", request.Email);
            
            return new RegisterResponse(true, token, "Регистрация успешна", userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
            return new RegisterResponse(false, null, "Ошибка регистрации", null);
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Попытка получить из Redis кеша (быстро!)
            var cachedUser = await _authCacheService.GetCachedUserAsync(userId, cancellationToken);
            if (cachedUser != null)
            {
                _logger.LogDebug("User found in Redis cache: {UserId}", userId);
                return MapToDto(cachedUser);
            }

            // 2. Получить из MongoDB (медленнее)
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return null;
            }

            // 3. Обновить кеш в Redis
            await _authCacheService.CacheUserAsync(user, cancellationToken);

            return MapToDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
            return null;
        }
    }

    public async Task<UserDto?> UpdateProfileAsync(string userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Получить из MongoDB
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return null;
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;

            // Сохранить в MongoDB
            await _userRepository.UpdateAsync(user, cancellationToken);

            // Инвалидировать кеш в Redis
            await _authCacheService.InvalidateUserCacheAsync(userId, cancellationToken);
            
            _logger.LogInformation("User profile updated in MongoDB: {UserId}", userId);
            return MapToDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile: {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return false;
            }

            // Получить из MongoDB
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return false;
            }

            // Проверить текущий пароль
            if (!_passwordHashService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                _logger.LogWarning("Change password failed: Invalid current password - {UserId}", userId);
                return false;
            }

            // Обновить пароль и СОХРАНИТЬ в MongoDB
            user.PasswordHash = _passwordHashService.HashPassword(request.NewPassword);
            await _userRepository.UpdateAsync(user, cancellationToken);

            // Инвалидировать кеш в Redis
            await _authCacheService.InvalidateUserCacheAsync(userId, cancellationToken);
            
            _logger.LogInformation("User password changed in MongoDB: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            return false;
        }
    }

    private UserDto MapToDto(User user)
    {
        return new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            user.CreatedAt,
            user.LastLogin,
            user.EnrolledCourses.Count,
            user.CompletedCourses.Count
        );
    }
}
