using SteelDesignerEngineer.Contracts.Constants;
using SteelDesignerEngineer.Contracts.Messages;
using SteelDesignerEngineer.ApiGateway.Messaging;

namespace SteelDesignerEngineer.ApiGateway.Clients;

/// <summary>
/// Auth Service client implementation using RabbitMQ RPC
/// </summary>
public class AuthServiceClient : IAuthServiceClient
{
    private readonly RabbitMqRpcClient _rpcClient;
    private readonly ILogger<AuthServiceClient> _logger;

    public AuthServiceClient(RabbitMqRpcClient rpcClient, ILogger<AuthServiceClient> logger)
    {
        _rpcClient = rpcClient;
        _logger = logger;
    }

    public async Task<RegisterUserResponse?> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calling Auth Service: Register user {Email}", request.Email);
        return await _rpcClient.CallAsync<RegisterUserRequest, RegisterUserResponse>(
            MessageQueues.AuthServiceRequestQueue,
            request,
            MessageQueues.RegisterUserType,
            TimeSpan.FromSeconds(30)
        );
    }

    public async Task<LoginUserResponse?> LoginAsync(LoginUserRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calling Auth Service: Login user {Email}", request.Email);
        return await _rpcClient.CallAsync<LoginUserRequest, LoginUserResponse>(
            MessageQueues.AuthServiceRequestQueue,
            request,
            MessageQueues.LoginUserType,
            TimeSpan.FromSeconds(30)
        );
    }

    public async Task<GetUserResponse?> GetUserAsync(GetUserRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Calling Auth Service: Get user {UserId}", request.UserId);
        return await _rpcClient.CallAsync<GetUserRequest, GetUserResponse>(
            MessageQueues.AuthServiceRequestQueue,
            request,
            MessageQueues.GetUserType,
            TimeSpan.FromSeconds(10)
        );
    }

    public async Task<UpdateProfileResponse?> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calling Auth Service: Update profile for user {UserId}", request.UserId);
        return await _rpcClient.CallAsync<UpdateProfileRequest, UpdateProfileResponse>(
            MessageQueues.AuthServiceRequestQueue,
            request,
            MessageQueues.UpdateProfileType,
            TimeSpan.FromSeconds(30)
        );
    }

    public async Task<ChangePasswordResponse?> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calling Auth Service: Change password for user {UserId}", request.UserId);
        return await _rpcClient.CallAsync<ChangePasswordRequest, ChangePasswordResponse>(
            MessageQueues.AuthServiceRequestQueue,
            request,
            MessageQueues.ChangePasswordType,
            TimeSpan.FromSeconds(30)
        );
    }

    public async Task<OAuthLoginResponse?> OAuthLoginAsync(OAuthLoginRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calling Auth Service: OAuth login {Email} via {Provider}", request.Email, request.Provider);
        return await _rpcClient.CallAsync<OAuthLoginRequest, OAuthLoginResponse>(
            MessageQueues.AuthServiceRequestQueue,
            request,
            MessageQueues.OAuthLoginType,
            TimeSpan.FromSeconds(30)
        );
    }
}

/// <summary>
/// Session Service client implementation using RabbitMQ RPC
/// </summary>
public class SessionServiceClient : ISessionServiceClient
{
    private readonly RabbitMqRpcClient _rpcClient;
    private readonly ILogger<SessionServiceClient> _logger;

    public SessionServiceClient(RabbitMqRpcClient rpcClient, ILogger<SessionServiceClient> logger)
    {
        _rpcClient = rpcClient;
        _logger = logger;
    }

    public async Task<CreateSessionResponse?> CreateSessionAsync(CreateSessionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calling Session Service: Create session for user {UserId}", request.UserId);
        return await _rpcClient.CallAsync<CreateSessionRequest, CreateSessionResponse>(
            MessageQueues.SessionServiceRequestQueue,
            request,
            MessageQueues.CreateSessionType,
            TimeSpan.FromSeconds(30)
        );
    }

    public async Task<ValidateSessionResponse?> ValidateSessionAsync(ValidateSessionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Calling Session Service: Validate session {SessionId}", request.SessionId);
        return await _rpcClient.CallAsync<ValidateSessionRequest, ValidateSessionResponse>(
            MessageQueues.SessionServiceRequestQueue,
            request,
            MessageQueues.ValidateSessionType,
            TimeSpan.FromSeconds(5)
        );
    }

    public async Task<DeleteSessionResponse?> DeleteSessionAsync(DeleteSessionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calling Session Service: Delete session {SessionId}", request.SessionId);
        return await _rpcClient.CallAsync<DeleteSessionRequest, DeleteSessionResponse>(
            MessageQueues.SessionServiceRequestQueue,
            request,
            MessageQueues.DeleteSessionType,
            TimeSpan.FromSeconds(10)
        );
    }

    public async Task<ExtendSessionResponse?> ExtendSessionAsync(ExtendSessionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Calling Session Service: Extend session {SessionId}", request.SessionId);
        return await _rpcClient.CallAsync<ExtendSessionRequest, ExtendSessionResponse>(
            MessageQueues.SessionServiceRequestQueue,
            request,
            MessageQueues.ExtendSessionType,
            TimeSpan.FromSeconds(5)
        );
    }

    public async Task<GetUserSessionsResponse?> GetUserSessionsAsync(GetUserSessionsRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Calling Session Service: Get sessions for user {UserId}", request.UserId);
        return await _rpcClient.CallAsync<GetUserSessionsRequest, GetUserSessionsResponse>(
            MessageQueues.SessionServiceRequestQueue,
            request,
            MessageQueues.GetUserSessionsType,
            TimeSpan.FromSeconds(10)
        );
    }
}

/// <summary>
/// Page Content Service client implementation using RabbitMQ RPC
/// </summary>
public class PageContentServiceClient : IPageContentServiceClient
{
    private readonly RabbitMqRpcClient _rpcClient;
    private readonly ILogger<PageContentServiceClient> _logger;

    public PageContentServiceClient(RabbitMqRpcClient rpcClient, ILogger<PageContentServiceClient> logger)
    {
        _rpcClient = rpcClient;
        _logger = logger;
    }

    public async Task<GetPageByNameResponse?> GetPageByNameAsync(GetPageByNameRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Calling Page Content Service: Get page by name {PageName}", request.PageName);
        return await _rpcClient.CallAsync<GetPageByNameRequest, GetPageByNameResponse>(
            MessageQueues.PageContentServiceRequestQueue,
            request,
            MessageQueues.GetPageByNameType,
            TimeSpan.FromSeconds(10)
        );
    }

    public async Task<GetPageByIdResponse?> GetPageByIdAsync(GetPageByIdRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Calling Page Content Service: Get page by ID {PageId}", request.PageId);
        return await _rpcClient.CallAsync<GetPageByIdRequest, GetPageByIdResponse>(
            MessageQueues.PageContentServiceRequestQueue,
            request,
            MessageQueues.GetPageByIdType,
            TimeSpan.FromSeconds(10)
        );
    }

    public async Task<GetAllPagesResponse?> GetAllPagesAsync(GetAllPagesRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Calling Page Content Service: Get all pages");
        return await _rpcClient.CallAsync<GetAllPagesRequest, GetAllPagesResponse>(
            MessageQueues.PageContentServiceRequestQueue,
            request,
            MessageQueues.GetAllPagesType,
            TimeSpan.FromSeconds(30)
        );
    }

    public async Task<CreatePageResponse?> CreatePageAsync(CreatePageRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calling Page Content Service: Create page {PageName}", request.PageName);
        return await _rpcClient.CallAsync<CreatePageRequest, CreatePageResponse>(
            MessageQueues.PageContentServiceRequestQueue,
            request,
            MessageQueues.CreatePageType,
            TimeSpan.FromSeconds(30)
        );
    }

    public async Task<UpdatePageResponse?> UpdatePageAsync(UpdatePageRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calling Page Content Service: Update page {PageId}", request.PageId);
        return await _rpcClient.CallAsync<UpdatePageRequest, UpdatePageResponse>(
            MessageQueues.PageContentServiceRequestQueue,
            request,
            MessageQueues.UpdatePageType,
            TimeSpan.FromSeconds(30)
        );
    }

    public async Task<DeletePageResponse?> DeletePageAsync(DeletePageRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calling Page Content Service: Delete page {PageName}", request.PageName);
        return await _rpcClient.CallAsync<DeletePageRequest, DeletePageResponse>(
            MessageQueues.PageContentServiceRequestQueue,
            request,
            MessageQueues.DeletePageType,
            TimeSpan.FromSeconds(30)
        );
    }
}
