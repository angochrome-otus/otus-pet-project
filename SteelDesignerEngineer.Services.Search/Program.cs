using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using RabbitMQ.Client;
using SteelDesignerEngineer.Services.Search;
using SteelDesignerEngineer.Services.Search.Messaging;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/search-service-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog();

    builder.Services.AddHttpClient("search", client =>
    {
        // timeouts are controlled by HttpClientHandler / per-request; keep default here
    });

    builder.Services.AddSingleton<IConnection>(_ =>
    {
        var cs = builder.Configuration.GetConnectionString("RabbitMQ")
                 ?? throw new InvalidOperationException("ConnectionStrings:RabbitMQ is required");
        return RabbitMqConnectionFactory.Create(cs);
    });

    builder.Services.AddHostedService<SearchServiceWorker>();

    var host = builder.Build();
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Search Service terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
