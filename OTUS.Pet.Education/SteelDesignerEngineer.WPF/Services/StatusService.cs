using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace SteelDesignerEngineer.WPF.Services;

/// <summary>
/// Implementation of status management service
/// SRP: Centralized status and message handling
/// </summary>
public class StatusService : IStatusService, INotifyPropertyChanged
{
    private string _statusMessage = "Ready";
    private string _statusIcon = "CheckCircle";
    private bool _isLoading;
    private string _loadingMessage = "Loading...";
    private Brush _serverStatusColor = Brushes.Red;
    private string _serverUrl = "localhost";

    public event EventHandler? StatusChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
                OnStatusChanged();
            }
        }
    }

    public string StatusIcon
    {
        get => _statusIcon;
        private set
        {
            if (_statusIcon != value)
            {
                _statusIcon = value;
                OnPropertyChanged();
                OnStatusChanged();
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged();
                OnStatusChanged();
            }
        }
    }

    public string LoadingMessage
    {
        get => _loadingMessage;
        private set
        {
            if (_loadingMessage != value)
            {
                _loadingMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public Brush ServerStatusColor
    {
        get => _serverStatusColor;
        private set
        {
            if (_serverStatusColor != value)
            {
                _serverStatusColor = value;
                OnPropertyChanged();
                OnStatusChanged();
            }
        }
    }

    public string ServerUrl
    {
        get => _serverUrl;
        private set
        {
            if (_serverUrl != value)
            {
                _serverUrl = value;
                OnPropertyChanged();
            }
        }
    }

    public void SetStatus(string message, string icon = "CheckCircle")
    {
        StatusMessage = message;
        StatusIcon = icon;
    }

    public void SetLoading(bool isLoading, string message = "Loading...")
    {
        IsLoading = isLoading;
        LoadingMessage = message;
        if (isLoading)
        {
            StatusIcon = "Loading";
        }
    }

    public void SetServerStatus(bool isAvailable, string url)
    {
        ServerStatusColor = isAvailable ? Brushes.Green : 
                           string.IsNullOrEmpty(url) ? Brushes.Red : Brushes.Orange;
        ServerUrl = url;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected virtual void OnStatusChanged()
    {
        StatusChanged?.Invoke(this, EventArgs.Empty);
    }
}
