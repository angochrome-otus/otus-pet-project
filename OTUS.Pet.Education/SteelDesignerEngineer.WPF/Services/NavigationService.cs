using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace SteelDesignerEngineer.WPF.Services;

/// <summary>
/// Implementation of navigation service
/// SRP: Centralized navigation logic
/// DIP: Depends on IWebBrowser abstraction
/// OCP: Portal path configurable through IConfiguration
/// </summary>
public class NavigationService : INavigationService, INotifyPropertyChanged
{
    private readonly string _apiBaseUrl;
    private readonly IStatusService _statusService;
    private readonly IApiService _apiService;
    private readonly CookieContainer _cookieContainer;
    private IWebBrowser? _browser;
    private string _currentUrl = string.Empty;

    public event EventHandler? NavigationChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public NavigationService(
        IStatusService statusService, 
        IApiService apiService,
        IConfiguration configuration,
        CookieContainer cookieContainer)
    {
        _statusService = statusService;
        _apiService = apiService;
        _cookieContainer = cookieContainer;
        
        // Use local or production API based on settings
        var useLocalApi = configuration.GetValue<bool>("ApiSettings:UseLocalApi");
        _apiBaseUrl = useLocalApi
            ? configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001"
            : configuration["ApiSettings:ProductionUrl"] ?? "https://steel-designer-engineer.ru";
    }

    public string CurrentUrl
    {
        get => _currentUrl;
        private set
        {
            if (_currentUrl != value)
            {
                _currentUrl = value;
                OnPropertyChanged();
                NavigationChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public bool CanGoBack => _browser?.CanGoBack ?? false;
    public bool CanGoForward => _browser?.CanGoForward ?? false;

    public void SetBrowser(IWebBrowser browser)
    {
        _browser = browser ?? throw new ArgumentNullException(nameof(browser));
        _browser.NavigationStarting += Browser_NavigationStarting;
        _browser.NavigationCompleted += Browser_NavigationCompleted;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _statusService.SetLoading(true, "Initializing browser...");

            if (_browser == null)
            {
                _statusService.SetStatus("Browser not set!", "AlertCircle");
                return;
            }

            // Ensure browser core is ready
            await _browser.EnsureCoreAsync();
            
            _statusService.SetStatus("Browser initialized", "CheckCircle");

            // Check if API is available
            _statusService.SetLoading(true, "Checking API server...");
            var isAvailable = await _apiService.CheckApiAvailabilityAsync();
            
            if (isAvailable)
            {
                _statusService.SetStatus("API server is ready", "CheckCircle");
                
                // Sync cookies from HttpClient to WebView2
                await SyncCookiesFromHttpClientToWebViewAsync();
                
                // Navigate to home page
                Navigate("index.html");
                _statusService.SetStatus("Ready", "CheckCircle");
            }
            else
            {
                _statusService.SetStatus("?? API server is not available - check connection", "AlertCircle");
            }
        }
        catch (Exception ex)
        {
            _statusService.SetStatus($"Init error: {ex.Message}", "AlertCircle");
            System.Diagnostics.Debug.WriteLine($"[NavigationService] Init error: {ex}");
        }
        finally
        {
            _statusService.SetLoading(false);
        }
    }

    public void Navigate(string page)
    {
        if (string.IsNullOrWhiteSpace(page) || _browser == null)
        {
            System.Diagnostics.Debug.WriteLine($"[NavigationService] Cannot navigate: page={page}, browser={_browser != null}");
            return;
        }

        try
        {
            // Navigate to API server URL instead of local file
            var url = page.StartsWith("http") 
                ? page 
                : $"{_apiBaseUrl}/{page.TrimStart('/')}";
            
            var uri = new Uri(url);
            
            System.Diagnostics.Debug.WriteLine($"[NavigationService] Navigating to: {url}");
            
            _browser.Source = uri;
            CurrentUrl = page;
            _statusService.SetStatus($"Loading {page}...");
        }
        catch (Exception ex)
        {
            _statusService.SetStatus($"Navigation error: {ex.Message}", "AlertCircle");
            System.Diagnostics.Debug.WriteLine($"[NavigationService] Navigation error: {ex}");
        }
    }

    public void GoBack()
    {
        _browser?.GoBack();
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CanGoForward));
    }

    public void GoForward()
    {
        _browser?.GoForward();
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CanGoForward));
    }

    public void Refresh()
    {
        _browser?.Reload();
        _statusService.SetStatus("Refreshing...");
    }

    private void Browser_NavigationStarting(object? sender, NavigationEventArgs e)
    {
        _statusService.SetLoading(true, "Loading page...");
    }

    private async void Browser_NavigationCompleted(object? sender, NavigationCompletedEventArgs e)
    {
        _statusService.SetLoading(false);
        
        if (e.IsSuccess)
        {
            _statusService.SetStatus("Page loaded", "CheckCircle");
            
            // Sync cookies from WebView2 to HttpClient after navigation
            await SyncCookiesFromWebViewToHttpClientAsync();
        }
        else
        {
            _statusService.SetStatus($"Load error: {e.ErrorMessage}", "AlertCircle");
        }
        
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CanGoForward));
    }

    /// <summary>
    /// Синхронизирует cookies из HttpClient в WebView2
    /// </summary>
    private async Task SyncCookiesFromHttpClientToWebViewAsync()
    {
        if (_browser == null) return;

        try
        {
            var cookieManager = _browser.GetCookieManager();
            if (cookieManager == null) return;

            var baseUri = new Uri(_apiBaseUrl);
            var cookies = _cookieContainer.GetCookies(baseUri);

            foreach (Cookie cookie in cookies)
            {
                var webViewCookie = cookieManager.CreateCookie(
                    cookie.Name,
                    cookie.Value,
                    cookie.Domain ?? baseUri.Host,
                    cookie.Path ?? "/"
                );

                if (cookie.Expires != default && cookie.Expires > DateTime.Now)
                {
                    webViewCookie.Expires = cookie.Expires.ToUniversalTime();
                }

                cookieManager.AddOrUpdateCookie(webViewCookie);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Cookie sync error (HttpClient -> WebView2): {ex.Message}");
        }
    }

    /// <summary>
    /// Синхронизирует cookies из WebView2 в HttpClient
    /// </summary>
    private async Task SyncCookiesFromWebViewToHttpClientAsync()
    {
        if (_browser == null) return;

        try
        {
            var cookieManager = _browser.GetCookieManager();
            if (cookieManager == null) return;

            var baseUri = new Uri(_apiBaseUrl);
            var webViewCookies = await cookieManager.GetCookiesAsync(baseUri.ToString());

            foreach (var webViewCookie in webViewCookies)
            {
                var cookie = new Cookie(
                    webViewCookie.Name,
                    webViewCookie.Value,
                    webViewCookie.Path,
                    webViewCookie.Domain
                );

                if (webViewCookie.Expires != default(DateTime) && webViewCookie.Expires > DateTime.MinValue)
                {
                    cookie.Expires = webViewCookie.Expires;
                }

                _cookieContainer.Add(baseUri, cookie);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Cookie sync error (WebView2 -> HttpClient): {ex.Message}");
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
