using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace SteelDesignerEngineer.Services.Session.Startup;

public class SessionDatabaseInitializer : BackgroundService
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<SessionDatabaseInitializer> _logger;

    public SessionDatabaseInitializer(IMongoDatabase database, ILogger<SessionDatabaseInitializer> logger)
    {
        _database = database;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Session DB init: ensuring collections...");

            var collections = await _database.ListCollectionNames().ToListAsync(stoppingToken);

            const string historyCollection = "sessionHistory";
            if (!collections.Contains(historyCollection))
            {
                await _database.CreateCollectionAsync(historyCollection, cancellationToken: stoppingToken);
                _logger.LogInformation("Created collection: {Collection}", historyCollection);
            }

            _logger.LogInformation("Session DB init completed");
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Session DB init failed");
        }
    }
}
