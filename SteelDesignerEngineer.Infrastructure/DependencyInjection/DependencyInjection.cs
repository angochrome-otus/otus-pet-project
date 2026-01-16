using System;
using SteelDesignerEngineer.Domain.Repositories;
using SteelDesignerEngineer.Domain.Services;
using SteelDesignerEngineer.Infrastructure.Caching;
using SteelDesignerEngineer.Infrastructure.Configuration;
using SteelDesignerEngineer.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;
using SteelDesignerEngineer.Infrastructure.Messaging;
using SteelDesignerEngineer.Application.Services;
using SteelDesignerEngineer.Application.Interfaces;
using Microsoft.Extensions.Logging;
using SteelDesignerEngineer.Data.DependencyInjection;
using SteelDesignerEngineer.Infrastructure.BackgroundServices;

namespace SteelDesignerEngineer.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Clean Architecture: Infrastructure Layer - Configuration of external dependencies
    /// SOLID: Dependency Inversion Principle (DIP)
    /// All external dependencies (MongoDB, Redis, RabbitMQ) are encapsulated
    /// 
    /// MICROSERVICES READY: Supports both monolithic and microservices architecture
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<ConnectionStringsSettings>(
                configuration.GetSection("ConnectionStrings"));
            services.Configure<DatabaseSettings>(
                configuration.GetSection("DatabaseSettings"));
            services.Configure<CacheSettings>(
                configuration.GetSection("CacheSettings"));

            // ==========================================
            // MongoDB - Configuration
            // ==========================================

            services.AddSingleton<IMongoClient>(sp =>
            {
                var connectionString = configuration.GetConnectionString("MongoDB");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("MongoDB connection string is not configured. Please set 'ConnectionStrings:MongoDB' in configuration.");
                }

                return new MongoClient(connectionString);
            });

            services.AddSingleton<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var databaseName = configuration.GetValue<string>("DatabaseSettings:DatabaseName");
                if (string.IsNullOrWhiteSpace(databaseName))
                {
                    throw new InvalidOperationException("MongoDB database name is not configured. Please set 'DatabaseSettings:DatabaseName' in configuration.");
                }

                return client.GetDatabase(databaseName);
            });

            services.AddSingleton<IMongoDbConnection>(sp =>
            {
                var database = sp.GetRequiredService<IMongoDatabase>();
                var mongoDbConnectionType = Type.GetType("SteelDesignerEngineer.Data.MongoDB.MongoDbConnection, SteelDesignerEngineer.Data");
                if (mongoDbConnectionType == null)
                {
                    throw new InvalidOperationException("Could not find MongoDbConnection type. Make sure SteelDesignerEngineer.Data is referenced in the startup project.");
                }
                return (IMongoDbConnection)Activator.CreateInstance(mongoDbConnectionType, database)!;
            });

            // ==========================================
            // Redis
            // ==========================================

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var connectionString = configuration.GetConnectionString("Redis");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("Redis connection string is not configured. Please set 'ConnectionStrings:Redis' in configuration.");
                }

                return ConnectionMultiplexer.Connect(connectionString);
            });

            services.AddSingleton<IRedisConnection, RedisConnection>();

            // ==========================================
            // RabbitMQ - Supports both Monolithic and Microservices
            // ==========================================

            services.AddSingleton<Lazy<RabbitMQ.Client.IConnection>>(sp =>
            {
                return new Lazy<RabbitMQ.Client.IConnection>(() =>
                {
                    var connectionString = configuration.GetConnectionString("RabbitMQ");
                    if (string.IsNullOrWhiteSpace(connectionString))
                    {
                        throw new InvalidOperationException("RabbitMQ connection string is not configured. Please set 'ConnectionStrings:RabbitMQ' in configuration.");
                    }

                    var factory = new RabbitMQ.Client.ConnectionFactory
                    {
                        Uri = new Uri(connectionString),
                        AutomaticRecoveryEnabled = true,
                        NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                        RequestedHeartbeat = TimeSpan.FromSeconds(60)
                    };

                    var logger = sp.GetRequiredService<ILogger<RabbitMqConnection>>();
                    logger.LogInformation("Creating RabbitMQ connection (Lazy Loading)");
                    
                    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
                });
            });

            // Register actual connection for backwards compatibility
            services.AddSingleton<RabbitMQ.Client.IConnection>(sp => 
                sp.GetRequiredService<Lazy<RabbitMQ.Client.IConnection>>().Value);

            services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
            services.AddSingleton<IRabbitMqConnectionExtended>(sp => 
                (IRabbitMqConnectionExtended)sp.GetRequiredService<IRabbitMqConnection>());

            // ==========================================
            // RabbitMQ RPC Client for Microservices
            // ==========================================
            
            services.AddSingleton<RabbitMqRpcClient>(sp =>
            {
                var connection = sp.GetRequiredService<RabbitMQ.Client.IConnection>();
                var logger = sp.GetRequiredService<ILogger<RabbitMqRpcClient>>();
                return new RabbitMqRpcClient(connection, logger);
            });

            // ==========================================
            // Domain Services - ISP: Register segregated interfaces
            // ==========================================

            services.AddSingleton<ICacheService, RedisCacheService>();
            
            // Register message serializer (OCP)
            services.AddSingleton<Infrastructure.Messaging.IMessageSerializer, Infrastructure.Messaging.JsonMessageSerializer>();
            
            // Register RabbitMqMessageBus as all message-related interfaces
            services.AddSingleton<RabbitMqMessageBus>();
            services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<RabbitMqMessageBus>());
            services.AddSingleton<IMessageConsumer>(sp => sp.GetRequiredService<RabbitMqMessageBus>());
            services.AddSingleton<IMessageBus>(sp => sp.GetRequiredService<RabbitMqMessageBus>());
            
            // ==========================================
            // RabbitMQ Queue Initialization
            // ==========================================
            
            services.AddHostedService<RabbitMqInitializationService>();
            
            // ==========================================
            // Auth Services
            // ==========================================
            
            services.AddSingleton<IJwtTokenService, JwtTokenService>();
            services.AddSingleton<IPasswordHashService, PasswordHashService>();
            services.AddSingleton<IAuthCacheService, AuthCacheService>();
            services.AddSingleton<IOAuthService, OAuthService>();
            services.AddSingleton<ISessionService, SessionService>();

            // ==========================================
            // OCP: Data layer repositories via extension method
            // ==========================================
            
            var tempProvider = services.BuildServiceProvider();
            var database = tempProvider.GetRequiredService<IMongoDatabase>();
            services.AddDataLayer(database);

            // Domain Services
            services.AddScoped<IPageContentService, PageContentService>();

            // HttpClient for OAuth
            services.AddHttpClient<IOAuthService, OAuthService>();

            return services;
        }

        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IPageContentApplicationService, PageContentApplicationService>();
            services.AddScoped<IAuthApplicationService, AuthApplicationService>();

            return services;
        }

        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            return services;
        }
    }
}