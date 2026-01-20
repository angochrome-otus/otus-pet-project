using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SteelDesignerEngineer.Contracts.Constants;
using SteelDesignerEngineer.Contracts.Messages;
using SteelDesignerEngineer.Services.Auth.Messaging;
using SteelDesignerEngineer.Services.Auth.Persistence;

namespace SteelDesignerEngineer.Services.Auth.Handlers;

/// <summary>
/// RPC Server for Auth Service
/// Handles authentication and user management requests
/// </summary>
internal sealed class AuthRpcServer : RabbitMqRpcServer
{
    private readonly IServiceProvider _serviceProvider;

    public AuthRpcServer(
        IConnection connection,
        string requestQueueName,
        IServiceProvider serviceProvider,
        ILogger logger)
        : base(connection, requestQueueName, logger)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task<object?> ProcessRequestAsync(string messageType, string requestBody, string correlationId)
    {
        using var scope = _serviceProvider.CreateScope();

        try
        {
            return messageType switch
            {
                MessageQueues.RegisterUserType => await HandleRegisterAsync(requestBody, scope),
                MessageQueues.LoginUserType => await HandleLoginAsync(requestBody, scope),
                MessageQueues.GetUserType => await HandleGetUserAsync(requestBody, scope),
                MessageQueues.UpdateProfileType => await HandleUpdateProfileAsync(requestBody, scope),
                MessageQueues.ChangePasswordType => await HandleChangePasswordAsync(requestBody, scope),
                MessageQueues.OAuthLoginType => await HandleOAuthLoginAsync(requestBody, scope),
                _ => throw new ArgumentException($"Unknown message type: {messageType}")
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing auth request: {MessageType}", messageType);
            return new { Success = false, Message = ex.Message };
        }
    }

    private static T Deserialize<T>(string json)
        => JsonSerializer.Deserialize<T>(json) ?? throw new InvalidOperationException("Invalid request body");

    private async Task<RegisterUserResponse> HandleRegisterAsync(string requestBody, IServiceScope scope)
    {
        var request = Deserialize<RegisterUserRequest>(requestBody);
        var repo = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var existingUser = await repo.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new RegisterUserResponse { Success = false, Message = "User with this email already exists" };
        }

        var requestedRole = (request.Role ?? string.Empty).Trim().ToLowerInvariant();
        var role = requestedRole switch
        {
            "teacher" => "teacher",
            "student" => "student",
            _ => "student"
        };

        var user = new UserDocument
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await repo.CreateAsync(user);

        return new RegisterUserResponse
        {
            Success = true,
            Message = "User registered successfully",
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role
        };
    }

    private async Task<LoginUserResponse> HandleLoginAsync(string requestBody, IServiceScope scope)
    {
        var request = Deserialize<LoginUserRequest>(requestBody);
        var repo = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var user = await repo.GetByEmailAsync(request.Email);
        if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return new LoginUserResponse { Success = false, Message = "Invalid email or password" };
        }

        if (!user.IsActive)
            return new LoginUserResponse { Success = false, Message = "User account is disabled" };

        user.LastLoginAt = DateTime.UtcNow;
        user.LastLoginIp = request.IpAddress;
        await repo.UpdateAsync(user);

        return new LoginUserResponse
        {
            Success = true,
            Message = "Login successful",
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            AvatarUrl = user.AvatarUrl
        };
    }

    private async Task<GetUserResponse> HandleGetUserAsync(string requestBody, IServiceScope scope)
    {
        var request = Deserialize<GetUserRequest>(requestBody);
        var repo = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var user = await repo.GetByIdAsync(request.UserId);
        if (user == null)
            return new GetUserResponse { Success = false };

        return new GetUserResponse
        {
            Success = true,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    private async Task<UpdateProfileResponse> HandleUpdateProfileAsync(string requestBody, IServiceScope scope)
    {
        var request = Deserialize<UpdateProfileRequest>(requestBody);
        var repo = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var user = await repo.GetByIdAsync(request.UserId);
        if (user == null)
            return new UpdateProfileResponse { Success = false, Message = "User not found" };

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        await repo.UpdateAsync(user);

        return new UpdateProfileResponse { Success = true, Message = "Profile updated successfully" };
    }

    private async Task<ChangePasswordResponse> HandleChangePasswordAsync(string requestBody, IServiceScope scope)
    {
        var request = Deserialize<ChangePasswordRequest>(requestBody);
        var repo = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var user = await repo.GetByIdAsync(request.UserId);
        if (user == null)
            return new ChangePasswordResponse { Success = false, Message = "User not found" };

        if (string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return new ChangePasswordResponse { Success = false, Message = "Current password is incorrect" };

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await repo.UpdateAsync(user);

        return new ChangePasswordResponse { Success = true, Message = "Password changed successfully" };
    }

    private async Task<OAuthLoginResponse> HandleOAuthLoginAsync(string requestBody, IServiceScope scope)
    {
        var request = Deserialize<OAuthLoginRequest>(requestBody);
        var repo = scope.ServiceProvider.GetRequiredService<UserRepository>();

        var user = await repo.GetByEmailAsync(request.Email);
        var isNewUser = false;

        if (user == null)
        {
            user = new UserDocument
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = null,
                Role = "student",
                AuthProvider = request.Provider,
                OAuthProviderId = request.ProviderId,
                AvatarUrl = request.AvatarUrl,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await repo.CreateAsync(user);
            isNewUser = true;
        }
        else
        {
            user.LastLoginAt = DateTime.UtcNow;
            user.LastLoginIp = request.IpAddress;
            await repo.UpdateAsync(user);
        }

        return new OAuthLoginResponse
        {
            Success = true,
            Message = "OAuth login successful",
            UserId = user.Id,
            IsNewUser = isNewUser
        };
    }
}
