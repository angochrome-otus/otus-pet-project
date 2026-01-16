using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SteelDesignerEngineer.Contracts.Constants;
using SteelDesignerEngineer.Contracts.Messages;
using SteelDesignerEngineer.Services.Session.Messaging;
using SteelDesignerEngineer.Services.Session.Persistence;

namespace SteelDesignerEngineer.Services.Session.Handlers;

internal sealed class SessionRpcServer : RabbitMqRpcServer
{
    private readonly IServiceProvider _serviceProvider;

    public SessionRpcServer(
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
                MessageQueues.CreateSessionType => await HandleCreateSessionAsync(requestBody, scope),
                MessageQueues.ValidateSessionType => await HandleValidateSessionAsync(requestBody, scope),
                MessageQueues.DeleteSessionType => await HandleDeleteSessionAsync(requestBody, scope),
                MessageQueues.ExtendSessionType => await HandleExtendSessionAsync(requestBody, scope),
                MessageQueues.GetUserSessionsType => await HandleGetUserSessionsAsync(requestBody, scope),
                _ => throw new ArgumentException($"Unknown message type: {messageType}")
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing session request: {MessageType}", messageType);
            return new { Success = false, Message = ex.Message };
        }
    }

    private static T Deserialize<T>(string json)
        => JsonSerializer.Deserialize<T>(json) ?? throw new InvalidOperationException("Invalid request body");

    private async Task<CreateSessionResponse> HandleCreateSessionAsync(string requestBody, IServiceScope scope)
    {
        var request = Deserialize<CreateSessionRequest>(requestBody);
        var store = scope.ServiceProvider.GetRequiredService<SessionStore>();
        var history = scope.ServiceProvider.GetRequiredService<SessionHistoryRepository>();

        var sessionId = Guid.NewGuid().ToString("N");
        var ttl = TimeSpan.FromMinutes(request.ExpirationMinutes <= 0 ? 1440 : request.ExpirationMinutes);

        await store.CreateAsync(
            sessionId,
            new SessionData
            {
                SessionId = sessionId,
                UserId = request.UserId,
                UserName = request.UserName,
                UserEmail = request.UserEmail,
                UserRole = request.UserRole,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent
            },
            ttl);

        await history.InsertAsync(new SessionHistoryDocument
        {
            UserId = request.UserId,
            SessionId = sessionId,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            CreatedAt = DateTime.UtcNow
        });

        return new CreateSessionResponse
        {
            Success = true,
            SessionId = sessionId,
            ExpiresAt = DateTime.UtcNow.Add(ttl)
        };
    }

    private async Task<ValidateSessionResponse> HandleValidateSessionAsync(string requestBody, IServiceScope scope)
    {
        var request = Deserialize<ValidateSessionRequest>(requestBody);
        var store = scope.ServiceProvider.GetRequiredService<SessionStore>();

        var session = await store.GetAsync(request.SessionId);
        if (session == null || string.IsNullOrEmpty(session.UserId))
        {
            return new ValidateSessionResponse { IsValid = false, Message = "Session not found or expired" };
        }

        await store.ExtendAsync(request.SessionId, TimeSpan.FromHours(24));

        return new ValidateSessionResponse
        {
            IsValid = true,
            UserId = session.UserId,
            UserName = session.UserName,
            UserEmail = session.UserEmail,
            UserRole = session.UserRole
        };
    }

    private async Task<DeleteSessionResponse> HandleDeleteSessionAsync(string requestBody, IServiceScope scope)
    {
        var request = Deserialize<DeleteSessionRequest>(requestBody);
        var store = scope.ServiceProvider.GetRequiredService<SessionStore>();

        await store.DeleteAsync(request.SessionId);

        return new DeleteSessionResponse { Success = true, Message = "Session deleted successfully" };
    }

    private async Task<ExtendSessionResponse> HandleExtendSessionAsync(string requestBody, IServiceScope scope)
    {
        var request = Deserialize<ExtendSessionRequest>(requestBody);
        var store = scope.ServiceProvider.GetRequiredService<SessionStore>();

        var ttl = TimeSpan.FromMinutes(request.ExpirationMinutes <= 0 ? 1440 : request.ExpirationMinutes);
        await store.ExtendAsync(request.SessionId, ttl);

        return new ExtendSessionResponse { Success = true, ExpiresAt = DateTime.UtcNow.Add(ttl) };
    }

    private async Task<GetUserSessionsResponse> HandleGetUserSessionsAsync(string requestBody, IServiceScope scope)
    {
        var request = Deserialize<GetUserSessionsRequest>(requestBody);
        var history = scope.ServiceProvider.GetRequiredService<SessionHistoryRepository>();

        var docs = await history.GetByUserIdAsync(request.UserId);

        return new GetUserSessionsResponse
        {
            Success = true,
            Sessions = docs.Select(d => new SessionInfo
            {
                SessionId = d.SessionId,
                LoginAt = d.CreatedAt,
                IpAddress = d.IpAddress,
                UserAgent = d.UserAgent,
                IsActive = false
            }).ToList()
        };
    }
}
