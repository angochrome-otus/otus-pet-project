using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SteelDesignerEngineer.Contracts.Constants;
using SteelDesignerEngineer.Services.PageContent.Handlers;

namespace SteelDesignerEngineer.Services.PageContent;

public class PageContentServiceWorker : BackgroundService
{
    private readonly ILogger<PageContentServiceWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnection _connection;
    private PageContentRpcServer? _rpcServer;

    public PageContentServiceWorker(
        ILogger<PageContentServiceWorker> logger,
        IServiceProvider serviceProvider,
        IConnection connection)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _connection = connection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PageContent Service Worker starting...");

        try
        {
            _rpcServer = new PageContentRpcServer(
                _connection,
                MessageQueues.PageContentServiceRequestQueue,
                _serviceProvider,
                _logger
            );

            await _rpcServer.StartAsync(stoppingToken);

            _logger.LogInformation("PageContent Service is now listening for requests");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("PageContent Service Worker is stopping due to cancellation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in PageContent Service Worker");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PageContent Service Worker stopping...");
        _rpcServer?.Dispose();
        await base.StopAsync(stoppingToken);
    }

    public override void Dispose()
    {
        _rpcServer?.Dispose();
        base.Dispose();
    }
}
