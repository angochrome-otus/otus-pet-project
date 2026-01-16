namespace SteelDesignerEngineer.Domain.Constants;

/// <summary>
/// RabbitMQ queue names for inter-layer communication
/// </summary>
public static class RabbitMqQueues
{
    // Authentication queues
    public const string AuthLoginRequest = "auth.login.request";
    public const string AuthLoginResponse = "auth.login.response";
    public const string AuthRegisterRequest = "auth.register.request";
    public const string AuthRegisterResponse = "auth.register.response";
    public const string AuthLogoutRequest = "auth.logout.request";
    
    // User management queues
    public const string UserGetRequest = "user.get.request";
    public const string UserGetResponse = "user.get.response";
    public const string UserUpdateRequest = "user.update.request";
    public const string UserUpdateResponse = "user.update.response";
    
    // Session management queues
    public const string SessionCreateRequest = "session.create.request";
    public const string SessionCreateResponse = "session.create.response";
    public const string SessionValidateRequest = "session.validate.request";
    public const string SessionValidateResponse = "session.validate.response";
    public const string SessionDeleteRequest = "session.delete.request";
    
    // Page content queues
    public const string PageGetRequest = "page.get.request";
    public const string PageGetResponse = "page.get.response";
    public const string PageUpdateRequest = "page.update.request";
    public const string PageUpdateResponse = "page.update.response";
    
    // Events
    public const string UserLoggedInEvent = "event.user.logged_in";
    public const string UserLoggedOutEvent = "event.user.logged_out";
    public const string UserRegisteredEvent = "event.user.registered";
}
