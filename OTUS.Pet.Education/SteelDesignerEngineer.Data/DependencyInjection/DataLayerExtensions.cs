using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SteelDesignerEngineer.Data.MongoDB;
using SteelDesignerEngineer.Domain.Repositories;

namespace SteelDesignerEngineer.Data.DependencyInjection;

/// <summary>
/// Dependency injection configuration for Data layer
/// OCP: Separate registration removes reflection-based coupling
/// SRP: Data layer manages its own dependencies
/// SESSION-BASED AUTHENTICATION ONLY - NO JWT/REFRESH TOKENS
/// </summary>
public static class DataLayerExtensions
{
    /// <summary>
    /// Register Data layer services and repositories
    /// </summary>
    public static IServiceCollection AddDataLayer(
        this IServiceCollection services,
        IMongoDatabase database)
    {
        // Register repositories with proper dependency injection
        services.AddScoped<IPageContentRepository>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<PageContentRepository>>();
            return new PageContentRepository(database, "PageContent", logger);
        });

        services.AddScoped<IUserRepository>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<UserRepository>>();
            return new UserRepository(database, logger);
        });

        services.AddScoped<ISessionHistoryRepository>(sp =>
        {
            return new SessionHistoryRepository(database);
        });

        return services;
    }
}
