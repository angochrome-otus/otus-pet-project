using SteelDesignerEngineer.Contracts.Messages;

namespace SteelDesignerEngineer.ApiGateway.Clients;

/// <summary>
/// Client interface for Auth Service communication
/// </summary>
public interface IAuthServiceClient
{
    Task<RegisterUserResponse?> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
    Task<LoginUserResponse?> LoginAsync(LoginUserRequest request, CancellationToken cancellationToken = default);
    Task<GetUserResponse?> GetUserAsync(GetUserRequest request, CancellationToken cancellationToken = default);
    Task<UpdateProfileResponse?> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<ChangePasswordResponse?> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<OAuthLoginResponse?> OAuthLoginAsync(OAuthLoginRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Client interface for Session Service communication
/// </summary>
public interface ISessionServiceClient
{
    Task<CreateSessionResponse?> CreateSessionAsync(CreateSessionRequest request, CancellationToken cancellationToken = default);
    Task<ValidateSessionResponse?> ValidateSessionAsync(ValidateSessionRequest request, CancellationToken cancellationToken = default);
    Task<DeleteSessionResponse?> DeleteSessionAsync(DeleteSessionRequest request, CancellationToken cancellationToken = default);
    Task<ExtendSessionResponse?> ExtendSessionAsync(ExtendSessionRequest request, CancellationToken cancellationToken = default);
    Task<GetUserSessionsResponse?> GetUserSessionsAsync(GetUserSessionsRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Client interface for Page Content Service communication
/// </summary>
public interface IPageContentServiceClient
{
    Task<GetPageByNameResponse?> GetPageByNameAsync(GetPageByNameRequest request, CancellationToken cancellationToken = default);
    Task<GetPageByIdResponse?> GetPageByIdAsync(GetPageByIdRequest request, CancellationToken cancellationToken = default);
    Task<GetAllPagesResponse?> GetAllPagesAsync(GetAllPagesRequest request, CancellationToken cancellationToken = default);
    Task<CreatePageResponse?> CreatePageAsync(CreatePageRequest request, CancellationToken cancellationToken = default);
    Task<UpdatePageResponse?> UpdatePageAsync(UpdatePageRequest request, CancellationToken cancellationToken = default);
    Task<DeletePageResponse?> DeletePageAsync(DeletePageRequest request, CancellationToken cancellationToken = default);
}
