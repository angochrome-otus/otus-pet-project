using Microsoft.Web.WebView2.Wpf;

namespace SteelDesignerEngineer.WPF.Services;

/// <summary>
/// Service for navigation operations
/// SRP: Handles only navigation-related functionality
/// DIP: Depends on IWebBrowser abstraction
/// </summary>
public interface INavigationService
{
    string CurrentUrl { get; }
    bool CanGoBack { get; }
    bool CanGoForward { get; }
    
    event EventHandler? NavigationChanged;
    
    void SetBrowser(IWebBrowser browser);
    Task InitializeAsync();
    void Navigate(string page);
    void GoBack();
    void GoForward();
    void Refresh();
}
