using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace SteelDesignerEngineer.Services.PageContent.Startup;

public sealed class PageContentDatabaseInitializer : BackgroundService
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<PageContentDatabaseInitializer> _logger;

    public PageContentDatabaseInitializer(IMongoDatabase database, ILogger<PageContentDatabaseInitializer> logger)
    {
        _database = database;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("PageContent DB init: ensuring collections and indexes...");

            var collections = await _database.ListCollectionNames().ToListAsync(stoppingToken);

            const string pagesCollectionName = "pageContents";
            if (!collections.Contains(pagesCollectionName))
            {
                await _database.CreateCollectionAsync(pagesCollectionName, cancellationToken: stoppingToken);
                _logger.LogInformation("Created collection: {Collection}", pagesCollectionName);
            }

            var pages = _database.GetCollection<dynamic>(pagesCollectionName);

            // Ensure unique index on PageName so GetByName / upserts remain consistent
            var pageNameIndex = new CreateIndexModel<dynamic>(
                Builders<dynamic>.IndexKeys.Ascending("PageName"),
                new CreateIndexOptions { Unique = true, Name = "ux_pagecontents_pagename" }
            );

            try
            {
                await pages.Indexes.CreateOneAsync(pageNameIndex, cancellationToken: stoppingToken);
            }
            catch (MongoCommandException ex) when (
                ex.Message.Contains("Index already exists", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                // ignore
            }

            _logger.LogInformation("PageContent DB init completed");
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PageContent DB init failed");
        }
    }
}
