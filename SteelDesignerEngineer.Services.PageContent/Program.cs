using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using MongoDB.Driver;
using RabbitMQ.Client;
using SteelDesignerEngineer.Services.PageContent;
using SteelDesignerEngineer.Services.PageContent.Messaging;
using SteelDesignerEngineer.Services.PageContent.Persistence;
using SteelDesignerEngineer.Services.PageContent.Startup;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/pagecontent-service-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("=".PadRight(60, '='));
    Log.Information("PageContent Service starting...");
    Log.Information("=".PadRight(60, '='));

    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog();

    builder.Services.AddSingleton<IMongoClient>(_ =>
    {
        var cs = builder.Configuration.GetConnectionString("MongoDB")
                 ?? throw new InvalidOperationException("ConnectionStrings:MongoDB is required");
        return new MongoClient(cs);
    });

    builder.Services.AddSingleton<IMongoDatabase>(sp =>
    {
        var client = sp.GetRequiredService<IMongoClient>();
        var dbName = builder.Configuration["DatabaseSettings:DatabaseName"] ?? "SteelDesignerEngineerDB";
        return client.GetDatabase(dbName);
    });

    builder.Services.AddScoped<PageContentRepository>();

    builder.Services.AddSingleton<IConnection>(_ =>
    {
        var cs = builder.Configuration.GetConnectionString("RabbitMQ")
                 ?? throw new InvalidOperationException("ConnectionStrings:RabbitMQ is required");
        return RabbitMqConnectionFactory.Create(cs);
    });

    // Register RPC Server
    builder.Services.AddHostedService<PageContentServiceWorker>();
    // Init database (collections/indexes)
    builder.Services.AddHostedService<PageContentDatabaseInitializer>();

    var host = builder.Build();

    Log.Information("PageContent Service configured successfully");
    Log.Information("Listening to RabbitMQ queue: pagecontent.service.requests");
    Log.Information("=".PadRight(60, '='));

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "PageContent Service terminated unexpectedly");
}
finally
{
    Log.Information("PageContent Service stopped");
    await Log.CloseAndFlushAsync();
}
