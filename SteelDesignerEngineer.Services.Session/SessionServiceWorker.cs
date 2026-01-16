using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SteelDesignerEngineer.Contracts.Constants;
using SteelDesignerEngineer.Services.Session.Handlers;

namespace SteelDesignerEngineer.Services.Session;

public class SessionServiceWorker : BackgroundService
{
    private readonly ILogger<SessionServiceWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnection _connection;
    private SessionRpcServer? _rpcServer;

    public SessionServiceWorker(
        ILogger<SessionServiceWorker> logger,
        IServiceProvider serviceProvider,
        IConnection connection)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _connection = connection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Session Service Worker starting...");

        try
        {
            _rpcServer = new SessionRpcServer(
                _connection,
                MessageQueues.SessionServiceRequestQueue,
                _serviceProvider,
                _logger
            );

            await _rpcServer.StartAsync(stoppingToken);

            _logger.LogInformation("Session Service is now listening for requests");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Session Service Worker is stopping due to cancellation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in Session Service Worker");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Session Service Worker stopping...");
        _rpcServer?.Dispose();
        await base.StopAsync(stoppingToken);
    }

    public override void Dispose()
    {
        _rpcServer?.Dispose();
        base.Dispose();
    }
}
