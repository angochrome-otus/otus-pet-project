using Microsoft.Extensions.Logging;
using SteelDesignerEngineer.Application.Interfaces;
using SteelDesignerEngineer.Domain.DTOs;
using SteelDesignerEngineer.Domain.Entities;
using SteelDesignerEngineer.Domain.Repositories;
using SteelDesignerEngineer.Domain.Services;

namespace SteelDesignerEngineer.Application.Services;

/// <summary>
/// Application Service для авторизации - Session-based authentication
/// Clean Architecture: Application Layer
/// MongoDB - источник истины, Redis - сессии и кеш
/// </summary>
public class AuthApplicationService : IAuthApplicationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IAuthCacheService _authCacheService;
    private readonly IOAuthService _oauthService;
    private readonly ILogger<AuthApplicationService> _logger;

    private const int MaxLoginAttempts = 5;

    public AuthApplicationService(
        IUserRepository userRepository,
        IPasswordHashService passwordHashService,
        IAuthCacheService authCacheService,
        IOAuthService oauthService,
        ILogger<AuthApplicationService> logger)
    {
        _userRepository = userRepository;
        _passwordHashService = passwordHashService;
        _authCacheService = authCacheService;
        _oauthService = oauthService;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        return await LoginAsync(request, null, cancellationToken);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            // Rate limiting
            var attempts = await _authCacheService.GetLoginAttemptsCountAsync(request.Email, cancellationToken);
            if (attempts >= MaxLoginAttempts)
            {
                _logger.LogWarning("Too many login attempts for {Email}: {Attempts}", request.Email, attempts);
                return new LoginResponse(false, $"Слишком много попыток входа. Попробуйте через 15 минут.", null);
            }

            // Find user in MongoDB
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null || !user.IsActive)
            {
                await _authCacheService.RecordLoginAttemptAsync(request.Email, cancellationToken);
                _logger.LogWarning("Login failed: User not found or inactive - {Email}", request.Email);
                return new LoginResponse(false, "Неверный email или пароль", null);
            }

            // Check if OAuth user trying to login with password
            if (user.AuthProvider != "local")
            {
                _logger.LogWarning("OAuth user {Email} trying to login with password", request.Email);
                return new LoginResponse(false, $"Используйте вход через {user.AuthProvider}", null);
            }

            // Verify password
            if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordHashService.VerifyPassword(request.Password, user.PasswordHash))
            {
                await _authCacheService.RecordLoginAttemptAsync(request.Email, cancellationToken);
                _logger.LogWarning("Login failed: Invalid password - {Email}", request.Email);
                return new LoginResponse(false, "Неверный email или пароль", null);
            }

            // Success - reset attempts
            await _authCacheService.ResetLoginAttemptsAsync(request.Email, cancellationToken);

            // Update lastLoginAt and IP in MongoDB
            user.LastLoginAt = DateTime.UtcNow;
            user.LastLoginIp = ipAddress;
            await _userRepository.UpdateAsync(user, cancellationToken);

            // Cache user in Redis
            await _authCacheService.CacheUserAsync(user, cancellationToken);

            var userDto = MapToDto(user);
            _logger.LogInformation("User logged in successfully: {Email}", request.Email);
            
            return new LoginResponse(true, "Успешный вход", userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return new LoginResponse(false, "Ошибка авторизации", null);
        }
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        return await RegisterAsync(request, null, cancellationToken);
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.Password != request.ConfirmPassword)
            {
                return new RegisterResponse(false, "Пароли не совпадают", null);
            }

            var exists = await _userRepository.ExistsAsync(request.Email, cancellationToken);
            if (exists)
            {
                _logger.LogWarning("Registration failed: Email already exists - {Email}", request.Email);
                return new RegisterResponse(false, "Email уже используется", null);
            }

            // Create user based on role
            User user = request.Role.ToLower() switch
            {
                "teacher" => new Teacher
                {
                    Email = request.Email,
                    PasswordHash = _passwordHashService.HashPassword(request.Password),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    LastLoginIp = ipAddress,
                    IsActive = true,
                    AuthProvider = "local",
                    TeachingCourses = new List<string>()
                },
                "admin" => new Admin
                {
                    Email = request.Email,
                    PasswordHash = _passwordHashService.HashPassword(request.Password),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    LastLoginIp = ipAddress,
                    IsActive = true,
                    AuthProvider = "local",
                    Permissions = new List<string> { "ManageUsers", "ManageCourses", "ViewReports", "SystemSettings" }
                },
                _ => new Student // Default to Student
                {
                    Email = request.Email,
                    PasswordHash = _passwordHashService.HashPassword(request.Password),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    LastLoginIp = ipAddress,
                    IsActive = true,
                    AuthProvider = "local",
                    EnrolledCourses = new List<string>(),
                    CompletedCourses = new List<string>()
                }
            };

            user = await _userRepository.CreateAsync(user, cancellationToken);

            // Cache in Redis
            await _authCacheService.CacheUserAsync(user, cancellationToken);

            var userDto = MapToDto(user);
            _logger.LogInformation("User registered successfully: {Email} as {Role}", request.Email, user.Role);
            
            return new RegisterResponse(true, "Регистрация успешна", userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
            return new RegisterResponse(false, "Ошибка регистрации", null);
        }
    }

    public async Task<OAuthLoginResponse> OAuthLoginAsync(OAuthUserInfo oauthUser, CancellationToken cancellationToken = default)
    {
        return await OAuthLoginAsync(oauthUser, null, cancellationToken);
    }

    public async Task<OAuthLoginResponse> OAuthLoginAsync(OAuthUserInfo oauthUser, string? ipAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByOAuthProviderAsync(oauthUser.Provider, oauthUser.ProviderId, cancellationToken)
                ?? await _userRepository.GetByEmailAsync(oauthUser.Email, cancellationToken);

            bool isNewUser = false;

            if (user == null)
            {
                // Create Student by default for OAuth users
                user = new Student
                {
                    Email = oauthUser.Email,
                    FirstName = oauthUser.FirstName,
                    LastName = oauthUser.LastName,
                    AuthProvider = oauthUser.Provider,
                    OAuthProviderId = oauthUser.ProviderId,
                    AvatarUrl = oauthUser.AvatarUrl,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    LastLoginIp = ipAddress,
                    EnrolledCourses = new List<string>(),
                    CompletedCourses = new List<string>()
                };

                user = await _userRepository.CreateAsync(user, cancellationToken);
                isNewUser = true;
                
                _logger.LogInformation("New OAuth user registered: {Email} via {Provider}", oauthUser.Email, oauthUser.Provider);
            }
            else
            {
                if (user.AuthProvider != oauthUser.Provider || user.OAuthProviderId != oauthUser.ProviderId)
                {
                    user.AuthProvider = oauthUser.Provider;
                    user.OAuthProviderId = oauthUser.ProviderId;
                }

                if (!string.IsNullOrEmpty(oauthUser.AvatarUrl))
                {
                    user.AvatarUrl = oauthUser.AvatarUrl;
                }

                user.LastLoginAt = DateTime.UtcNow;
                user.LastLoginIp = ipAddress;
                await _userRepository.UpdateAsync(user, cancellationToken);
                
                _logger.LogInformation("OAuth user logged in: {Email} via {Provider}", oauthUser.Email, oauthUser.Provider);
            }

            // Cache in Redis
            await _authCacheService.CacheUserAsync(user, cancellationToken);

            var userDto = MapToDto(user);
            
            return new OAuthLoginResponse(
                true,
                isNewUser ? "Регистрация через OAuth успешна" : "Вход через OAuth успешен",
                userDto,
                isNewUser
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OAuth login for email: {Email}", oauthUser.Email);
            return new OAuthLoginResponse(false, "Ошибка OAuth авторизации", null, false);
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Try Redis cache first
            var cachedUser = await _authCacheService.GetCachedUserAsync(userId, cancellationToken);
            if (cachedUser != null)
            {
                _logger.LogDebug("User found in Redis cache: {UserId}", userId);
                return MapToDto(cachedUser);
            }

            // Fallback to MongoDB
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return null;
            }

            // Update cache
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
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return null;
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;

            await _userRepository.UpdateAsync(user, cancellationToken);

            // Invalidate cache
            await _authCacheService.InvalidateUserCacheAsync(userId, cancellationToken);
            
            _logger.LogInformation("User profile updated: {UserId}", userId);
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

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return false;
            }

            if (user.AuthProvider != "local")
            {
                _logger.LogWarning("OAuth user {UserId} trying to change password", userId);
                return false;
            }

            if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordHashService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                _logger.LogWarning("Change password failed: Invalid current password - {UserId}", userId);
                return false;
            }

            user.PasswordHash = _passwordHashService.HashPassword(request.NewPassword);
            await _userRepository.UpdateAsync(user, cancellationToken);

            // Invalidate cache
            await _authCacheService.InvalidateUserCacheAsync(userId, cancellationToken);
            
            _logger.LogInformation("User password changed: {UserId}", userId);
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
        var baseDto = new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            user.CreatedAt,
            user.LastLoginAt,
            0, // Will be overridden for Student
            0  // Will be overridden for Student
        )
        {
            AuthProvider = user.AuthProvider,
            AvatarUrl = user.AvatarUrl,
            LastLoginIp = user.LastLoginIp
        };

        // Add role-specific properties
        return user switch
        {
            Student student => baseDto with
            {
                EnrolledCourses = student.EnrolledCourses,
                CompletedCourses = student.CompletedCourses,
                EnrolledCoursesCount = student.EnrolledCourses.Count,
                CompletedCoursesCount = student.CompletedCourses.Count,
                CurrentSemester = student.CurrentSemester,
                StudentIdNumber = student.StudentIdNumber
            },
            Teacher teacher => baseDto with
            {
                TeachingCourses = teacher.TeachingCourses,
                Specialization = teacher.Specialization,
                AcademicTitle = teacher.AcademicTitle,
                Bio = teacher.Bio
            },
            Admin admin => baseDto with
            {
                Permissions = admin.Permissions,
                Notes = admin.Notes
            },
            _ => baseDto
        };
    }
}
