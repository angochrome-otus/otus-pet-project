using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace SteelDesignerEngineer.WPF.Services;

/// <summary>
/// —ервис дл€ запуска и управлени€ локальным API сервером
/// </summary>
public class LocalApiService : ILocalApiService, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly IStatusService _statusService;
    private Process? _apiProcess;
    private readonly string _localApiUrl;
    private readonly string _projectPath;
    private readonly bool _shouldLaunch;

    public LocalApiService(IConfiguration configuration, IStatusService statusService)
    {
        _configuration = configuration;
        _statusService = statusService;
        
        _localApiUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";
        _shouldLaunch = _configuration.GetValue<bool>("ApiSettings:LaunchLocalApi");
        
        var relativePath = _configuration["ApiSettings:LocalApiProjectPath"] ?? "..\\SteelDesignerEngineer\\SteelDesignerEngineer.csproj";
        _projectPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath));
    }

    public bool IsRunning => _apiProcess != null && !_apiProcess.HasExited;

    public string GetLocalApiUrl() => _localApiUrl;

    public async Task<bool> StartAsync()
    {
        if (IsRunning)
        {
            _statusService.SetStatus("Local API already running", "CheckCircle");
            return true;
        }

        if (!_shouldLaunch)
        {
            _statusService.SetStatus("Local API launch disabled in settings", "Information");
            return false;
        }

        if (!File.Exists(_projectPath))
        {
            _statusService.SetStatus($"API project not found: {_projectPath}", "AlertCircle");
            return false;
        }

        try
        {
            _statusService.SetLoading(true, "Starting local API server...");

            var projectDirectory = Path.GetDirectoryName(_projectPath);
            
            _apiProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run --project \"{_projectPath}\"", // Removed --no-build to ensure fresh build
                    WorkingDirectory = projectDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = false, // Show window for debugging
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            _apiProcess.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    Debug.WriteLine($"[API] {e.Data}");
                }
            };

            _apiProcess.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    Debug.WriteLine($"[API ERROR] {e.Data}");
                }
            };

            _apiProcess.Start();
            _apiProcess.BeginOutputReadLine();
            _apiProcess.BeginErrorReadLine();

            _statusService.SetStatus("Waiting for API to start (10 seconds)...", "Information");

            // Wait longer for API to start (10 seconds)
            await Task.Delay(10000);

            // Check if process is still running
            if (!IsRunning)
            {
                _statusService.SetStatus("Failed to start local API - process exited", "AlertCircle");
                return false;
            }

            _statusService.SetStatus($"Local API started: {_localApiUrl}", "CheckCircle");
            return true;
        }
        catch (Exception ex)
        {
            _statusService.SetStatus($"Error starting API: {ex.Message}", "AlertCircle");
            Debug.WriteLine($"[API START ERROR] {ex}");
            return false;
        }
        finally
        {
            _statusService.SetLoading(false);
        }
    }

    public async Task StopAsync()
    {
        if (_apiProcess != null && !_apiProcess.HasExited)
        {
            try
            {
                _statusService.SetStatus("Stopping local API...", "Information");
                
                _apiProcess.Kill(true);
                await _apiProcess.WaitForExitAsync();
                
                _apiProcess.Dispose();
                _apiProcess = null;
                
                _statusService.SetStatus("Local API stopped", "CheckCircle");
            }
            catch (Exception ex)
            {
                _statusService.SetStatus($"Error stopping API: {ex.Message}", "AlertCircle");
            }
        }
    }

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
    }
}
