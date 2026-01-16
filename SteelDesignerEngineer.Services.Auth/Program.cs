using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using MongoDB.Driver;
using RabbitMQ.Client;
using SteelDesignerEngineer.Services.Auth;
using SteelDesignerEngineer.Services.Auth.Messaging;
using SteelDesignerEngineer.Services.Auth.Persistence;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/auth-service-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("=".PadRight(60, '='));
    Log.Information("Auth Service starting...");
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

    builder.Services.AddScoped<UserRepository>();

    builder.Services.AddSingleton<IConnection>(_ =>
    {
        var cs = builder.Configuration.GetConnectionString("RabbitMQ")
                 ?? throw new InvalidOperationException("ConnectionStrings:RabbitMQ is required");
        return RabbitMqConnectionFactory.Create(cs);
    });

    // Register RPC Server
    builder.Services.AddHostedService<AuthServiceWorker>();

    var host = builder.Build();

    Log.Information("Auth Service configured successfully");
    Log.Information("Listening to RabbitMQ queue: auth.service.requests");
    Log.Information("=".PadRight(60, '='));

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Auth Service terminated unexpectedly");
}
finally
{
    Log.Information("Auth Service stopped");
    await Log.CloseAndFlushAsync();
}
