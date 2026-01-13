using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;

namespace SteelDesignerEngineer.WPF.Services;

/// <summary>
/// Adapter for WebView2 to IWebBrowser interface
/// Adapter Pattern: Wraps WebView2 to match IWebBrowser interface
/// DIP: Allows NavigationService to depend on abstraction
/// </summary>
public class WebView2Adapter : IWebBrowser
{
    private readonly WebView2 _webView;

    public WebView2Adapter(WebView2 webView)
    {
        _webView = webView ?? throw new ArgumentNullException(nameof(webView));
        
        // Wire up events
        _webView.NavigationStarting += (s, e) => 
            NavigationStarting?.Invoke(this, new NavigationEventArgs { Uri = e.Uri != null ? new Uri(e.Uri) : null });
        
        _webView.NavigationCompleted += (s, e) => 
            NavigationCompleted?.Invoke(this, new NavigationCompletedEventArgs 
            { 
                IsSuccess = e.IsSuccess,
                ErrorMessage = e.WebErrorStatus.ToString()
            });
    }

    public Uri? Source 
    { 
        get => _webView.Source; 
        set => _webView.Source = value; 
    }

    public bool CanGoBack => _webView.CanGoBack;

    public bool CanGoForward => _webView.CanGoForward;

    public void GoBack() => _webView.GoBack();

    public void GoForward() => _webView.GoForward();

    public void Reload() => _webView.Reload();

    public async Task EnsureCoreAsync()
    {
        await _webView.EnsureCoreWebView2Async();
    }

    public CoreWebView2CookieManager? GetCookieManager()
    {
        return _webView.CoreWebView2?.CookieManager;
    }

    public event EventHandler<NavigationEventArgs>? NavigationStarting;
    public event EventHandler<NavigationCompletedEventArgs>? NavigationCompleted;
}
