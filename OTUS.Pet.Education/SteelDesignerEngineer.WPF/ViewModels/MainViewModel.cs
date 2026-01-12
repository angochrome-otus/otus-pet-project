using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Web.WebView2.Wpf;
using SteelDesignerEngineer.WPF.Services;
using SteelDesignerEngineer.WPF.Commands;
using System.IO;
using System.Diagnostics;

namespace SteelDesignerEngineer.WPF.ViewModels;

/// <summary>
/// ViewModel для главного окна - браузер портала
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly IApiService _apiService;
    private readonly string _portalPath;
    private WebView2? _webView;

    private string _currentUrl = string.Empty;
    private string _statusMessage = "Ready";
    private string _statusIcon = "CheckCircle";
    private bool _isLoading;
    private string _loadingMessage = "Loading...";
    private string _serverUrl = "localhost";
    private Brush _serverStatusColor = Brushes.Red;

    public MainViewModel(IApiService apiService)
    {
        _apiService = apiService;
        
        // Путь к wwwroot портала
        _portalPath = Path.Combine(
            Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.Parent!.FullName,
            "SteelDesignerEngineer", "wwwroot");

        // Команды
        NavigateCommand = new RelayCommand<string>(Navigate);
        GoBackCommand = new RelayCommand(() => GoBack(), () => CanGoBack());
        GoForwardCommand = new RelayCommand(() => GoForward(), () => CanGoForward());
        RefreshCommand = new RelayCommand(Refresh);
        OpenInExternalBrowserCommand = new RelayCommand(OpenInExternalBrowser);
    }

    #region Properties

    public string CurrentUrl
    {
        get => _currentUrl;
        set => SetProperty(ref _currentUrl, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string StatusIcon
    {
        get => _statusIcon;
        set => SetProperty(ref _statusIcon, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string LoadingMessage
    {
        get => _loadingMessage;
        set => SetProperty(ref _loadingMessage, value);
    }

    public string ServerUrl
    {
        get => _serverUrl;
        set => SetProperty(ref _serverUrl, value);
    }

    public Brush ServerStatusColor
    {
        get => _serverStatusColor;
        set => SetProperty(ref _serverStatusColor, value);
    }

    #endregion

    #region Commands

    public ICommand NavigateCommand { get; }
    public ICommand GoBackCommand { get; }
    public ICommand GoForwardCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand OpenInExternalBrowserCommand { get; }

    #endregion

    #region Public Methods

    public void SetWebView(WebView2 webView)
    {
        _webView = webView;
        _webView.NavigationStarting += WebView_NavigationStarting;
        _webView.NavigationCompleted += WebView_NavigationCompleted;
    }

    public async Task InitializeAsync()
    {
        try
        {
            IsLoading = true;
            LoadingMessage = "Initializing WebView2...";

            if (_webView != null)
            {
                await _webView.EnsureCoreWebView2Async();
            }

            // Проверка доступности сервера
            await CheckServerStatusAsync();

            // Загружаем главную страницу
            Navigate("index.html");

            StatusMessage = "Ready";
            StatusIcon = "CheckCircle";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Init error: {ex.Message}";
            StatusIcon = "AlertCircle";
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region Private Methods

    private void Navigate(string? page)
    {
        if (string.IsNullOrWhiteSpace(page) || _webView == null) return;

        try
        {
            var filePath = Path.Combine(_portalPath, page);
            
            if (File.Exists(filePath))
            {
                var uri = new Uri(filePath);
                _webView.Source = uri;
                CurrentUrl = page;
                StatusMessage = $"Loading {page}...";
            }
            else
            {
                StatusMessage = $"File not found: {page}";
                StatusIcon = "AlertCircle";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Navigation error: {ex.Message}";
            StatusIcon = "AlertCircle";
        }
    }

    private void GoBack()
    {
        _webView?.GoBack();
    }

    private bool CanGoBack()
    {
        return _webView?.CanGoBack ?? false;
    }

    private void GoForward()
    {
        _webView?.GoForward();
    }

    private bool CanGoForward()
    {
        return _webView?.CanGoForward ?? false;
    }

    private void Refresh()
    {
        _webView?.Reload();
        StatusMessage = "Refreshing...";
    }

    private void OpenInExternalBrowser()
    {
        if (!string.IsNullOrWhiteSpace(CurrentUrl))
        {
            var filePath = Path.Combine(_portalPath, CurrentUrl);
            if (File.Exists(filePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
        }
    }

    private async Task CheckServerStatusAsync()
    {
        try
        {
            var isAvailable = await _apiService.CheckApiAvailabilityAsync();
            
            if (isAvailable)
            {
                ServerStatusColor = Brushes.Green;
                ServerUrl = _apiService.GetBaseUrl();
            }
            else
            {
                ServerStatusColor = Brushes.Orange;
                ServerUrl = "Server unavailable";
            }
        }
        catch
        {
            ServerStatusColor = Brushes.Red;
            ServerUrl = "Connection error";
        }
    }

    private void WebView_NavigationStarting(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
    {
        IsLoading = true;
        LoadingMessage = "Loading page...";
        StatusIcon = "Loading";
    }

    private void WebView_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
    {
        IsLoading = false;
        
        if (e.IsSuccess)
        {
            StatusMessage = "Page loaded";
            StatusIcon = "CheckCircle";
        }
        else
        {
            StatusMessage = "Load error";
            StatusIcon = "AlertCircle";
        }
    }

    #endregion
}
