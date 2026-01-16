namespace SteelDesignerEngineer.Contracts.Constants;

/// <summary>
/// RabbitMQ queue and exchange names for microservices
/// Request-Reply Pattern: Each service has request and response queues
/// </summary>
public static class MessageQueues
{
    // ==========================================
    // Auth Service Queues
    // ==========================================
    
    public const string AuthServiceRequestQueue = "auth.service.requests";
    public const string AuthServiceResponseQueue = "auth.service.responses";
    
    // Message types for Auth Service
    public const string RegisterUserType = "auth.register";
    public const string LoginUserType = "auth.login";
    public const string GetUserType = "auth.getuser";
    public const string UpdateProfileType = "auth.updateprofile";
    public const string ChangePasswordType = "auth.changepassword";
    public const string OAuthLoginType = "auth.oauthlogin";
    
    // ==========================================
    // Session Service Queues
    // ==========================================
    
    public const string SessionServiceRequestQueue = "session.service.requests";
    public const string SessionServiceResponseQueue = "session.service.responses";
    
    // Message types for Session Service
    public const string CreateSessionType = "session.create";
    public const string ValidateSessionType = "session.validate";
    public const string DeleteSessionType = "session.delete";
    public const string ExtendSessionType = "session.extend";
    public const string GetUserSessionsType = "session.getusersessions";
    
    // ==========================================
    // Page Content Service Queues
    // ==========================================
    
    public const string PageContentServiceRequestQueue = "pagecontent.service.requests";
    public const string PageContentServiceResponseQueue = "pagecontent.service.responses";
    
    // Message types for Page Content Service
    public const string GetPageByNameType = "page.getbyname";
    public const string GetPageByIdType = "page.getbyid";
    public const string GetAllPagesType = "page.getall";
    public const string CreatePageType = "page.create";
    public const string UpdatePageType = "page.update";
    public const string DeletePageType = "page.delete";
    
    // ==========================================
    // Event Exchange (Pub-Sub Pattern)
    // ==========================================
    
    public const string EventExchange = "steeldesigner.events";
    public const string EventExchangeType = "topic";
    
    // Event routing keys
    public const string UserRegisteredEvent = "event.user.registered";
    public const string UserLoggedInEvent = "event.user.loggedin";
    public const string UserLoggedOutEvent = "event.user.loggedout";
    public const string UserProfileUpdatedEvent = "event.user.profileupdated";
    public const string PageCreatedEvent = "event.page.created";
    public const string PageUpdatedEvent = "event.page.updated";
    public const string PageDeletedEvent = "event.page.deleted";
    
    // ==========================================
    // Health Check Queue
    // ==========================================
    
    public const string HealthCheckQueue = "health.check";
    
    // ==========================================
    // Dead Letter Exchange (for failed messages)
    // ==========================================
    
    public const string DeadLetterExchange = "steeldesigner.dlx";
    public const string DeadLetterQueue = "steeldesigner.dlq";
}

/// <summary>
/// Service names for routing and identification
/// </summary>
public static class ServiceNames
{
    public const string ApiGateway = "ApiGateway";
    public const string AuthService = "AuthService";
    public const string SessionService = "SessionService";
    public const string PageContentService = "PageContentService";
}

/// <summary>
/// Message headers for routing and tracing
/// </summary>
public static class MessageHeaders
{
    public const string CorrelationId = "X-Correlation-Id";
    public const string MessageType = "X-Message-Type";
    public const string ServiceName = "X-Service-Name";
    public const string Timestamp = "X-Timestamp";
    public const string UserId = "X-User-Id";
    public const string ReplyTo = "X-Reply-To";
}
