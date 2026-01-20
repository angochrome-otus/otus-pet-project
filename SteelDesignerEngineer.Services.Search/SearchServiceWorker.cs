using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SteelDesignerEngineer.Contracts.Constants;
using SteelDesignerEngineer.Services.Search.Handlers;

namespace SteelDesignerEngineer.Services.Search;

public sealed class SearchServiceWorker : BackgroundService
{
    private readonly ILogger<SearchServiceWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnection _connection;
    private SearchRpcServer? _rpcServer;

    public SearchServiceWorker(ILogger<SearchServiceWorker> logger, IServiceProvider serviceProvider, IConnection connection)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _connection = connection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Search Service Worker starting...");

        _rpcServer = new SearchRpcServer(
            _connection,
            MessageQueues.SearchServiceRequestQueue,
            _serviceProvider,
            _logger);

        await _rpcServer.StartAsync(stoppingToken);
        _logger.LogInformation("Search Service is now listening for requests");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Search Service Worker stopping...");
        _rpcServer?.Dispose();
        await base.StopAsync(stoppingToken);
    }

    public override void Dispose()
    {
        _rpcServer?.Dispose();
        base.Dispose();
    }
}
