using System.Windows.Media;

namespace SteelDesignerEngineer.WPF.Services;

/// <summary>
/// Service for managing application status and messages
/// SRP: Handles only status-related operations
/// </summary>
public interface IStatusService
{
    string StatusMessage { get; }
    string StatusIcon { get; }
    bool IsLoading { get; }
    string LoadingMessage { get; }
    Brush ServerStatusColor { get; }
    string ServerUrl { get; }
    
    event EventHandler? StatusChanged;
    
    void SetStatus(string message, string icon = "CheckCircle");
    void SetLoading(bool isLoading, string message = "Loading...");
    void SetServerStatus(bool isAvailable, string url);
}
