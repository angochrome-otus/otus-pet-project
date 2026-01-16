using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SteelDesignerEngineer.Contracts.Constants;
using SteelDesignerEngineer.Services.Auth.Handlers;

namespace SteelDesignerEngineer.Services.Auth;

/// <summary>
/// Background worker that processes auth requests from RabbitMQ
/// </summary>
public class AuthServiceWorker : BackgroundService
{
    private readonly ILogger<AuthServiceWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnection _connection;
    private AuthRpcServer? _rpcServer;

    public AuthServiceWorker(
        ILogger<AuthServiceWorker> logger,
        IServiceProvider serviceProvider,
        IConnection connection)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _connection = connection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Auth Service Worker starting...");

        try
        {
            _rpcServer = new AuthRpcServer(
                _connection,
                MessageQueues.AuthServiceRequestQueue,
                _serviceProvider,
                _logger);

            await _rpcServer.StartAsync(stoppingToken);

            _logger.LogInformation("Auth Service is now listening for requests");

            // Keep running until cancellation is requested
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Auth Service Worker is stopping due to cancellation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in Auth Service Worker");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Auth Service Worker stopping...");
        _rpcServer?.Dispose();
        await base.StopAsync(stoppingToken);
    }

    public override void Dispose()
    {
        _rpcServer?.Dispose();
        base.Dispose();
    }
}
