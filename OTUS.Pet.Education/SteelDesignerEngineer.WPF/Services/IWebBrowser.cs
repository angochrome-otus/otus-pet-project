using Microsoft.Web.WebView2.Core;

namespace SteelDesignerEngineer.WPF.Services;

/// <summary>
/// Abstraction for web browser control
/// DIP: Decouples NavigationService from concrete WebView2 implementation
/// </summary>
public interface IWebBrowser
{
    /// <summary>
    /// Current navigation URI
    /// </summary>
    Uri? Source { get; set; }
    
    /// <summary>
    /// Can navigate backward
    /// </summary>
    bool CanGoBack { get; }
    
    /// <summary>
    /// Can navigate forward
    /// </summary>
    bool CanGoForward { get; }
    
    /// <summary>
    /// Navigate backward
    /// </summary>
    void GoBack();
    
    /// <summary>
    /// Navigate forward
    /// </summary>
    void GoForward();
    
    /// <summary>
    /// Reload current page
    /// </summary>
    void Reload();
    
    /// <summary>
    /// Initialize browser core
    /// </summary>
    Task EnsureCoreAsync();
    
    /// <summary>
    /// Get cookie manager for session synchronization
    /// </summary>
    CoreWebView2CookieManager? GetCookieManager();
    
    /// <summary>
    /// Event raised when navigation starts
    /// </summary>
    event EventHandler<NavigationEventArgs>? NavigationStarting;
    
    /// <summary>
    /// Event raised when navigation completes
    /// </summary>
    event EventHandler<NavigationCompletedEventArgs>? NavigationCompleted;
}

/// <summary>
/// Navigation event arguments
/// </summary>
public class NavigationEventArgs : EventArgs
{
    public Uri? Uri { get; init; }
}

/// <summary>
/// Navigation completed event arguments
/// </summary>
public class NavigationCompletedEventArgs : EventArgs
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
}
