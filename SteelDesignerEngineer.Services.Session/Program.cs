using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using MongoDB.Driver;
using RabbitMQ.Client;
using StackExchange.Redis;
using SteelDesignerEngineer.Services.Session;
using SteelDesignerEngineer.Services.Session.Messaging;
using SteelDesignerEngineer.Services.Session.Persistence;
using SteelDesignerEngineer.Services.Session.Startup;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/session-service-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("=".PadRight(60, '='));
    Log.Information("Session Service starting...");
    Log.Information("=".PadRight(60, '='));

    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog();

    // Redis
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    {
        var cs = builder.Configuration.GetConnectionString("Redis")
                 ?? throw new InvalidOperationException("ConnectionStrings:Redis is required");
        return ConnectionMultiplexer.Connect(cs);
    });

    // MongoDB (for session history)
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

    builder.Services.AddScoped<SessionHistoryRepository>();
    builder.Services.AddSingleton<SessionStore>();

    // RabbitMQ
    builder.Services.AddSingleton<IConnection>(_ =>
    {
        var cs = builder.Configuration.GetConnectionString("RabbitMQ")
                 ?? throw new InvalidOperationException("ConnectionStrings:RabbitMQ is required");
        return RabbitMqConnectionFactory.Create(cs);
    });

    // Register RPC Server
    builder.Services.AddHostedService<SessionServiceWorker>();
    // Init database (collections)
    builder.Services.AddHostedService<SessionDatabaseInitializer>();

    var host = builder.Build();

    Log.Information("Session Service configured successfully");
    Log.Information("Listening to RabbitMQ queue: session.service.requests");
    Log.Information("=".PadRight(60, '='));

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Session Service terminated unexpectedly");
}
finally
{
    Log.Information("Session Service stopped");
    await Log.CloseAndFlushAsync();
}
