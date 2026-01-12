using System;
using SteelDesignerEngineer.Domain.Repositories;
using SteelDesignerEngineer.Domain.Services;
using SteelDesignerEngineer.Infrastructure.Caching;
using SteelDesignerEngineer.Infrastructure.Configuration;
using SteelDesignerEngineer.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using RabbitMQ.Client;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;
using SteelDesignerEngineer.Data.MongoDB;
using SteelDesignerEngineer.Infrastructure.Messaging;
using SteelDesignerEngineer.Application.Services;
using SteelDesignerEngineer.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace SteelDesignerEngineer.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Класс для внедрения зависимостей в слое инфраструктуры
    /// Clean Architecture: Infrastructure Layer - Реализация внешних зависимостей
    /// SOLID: Dependency Inversion Principle (DIP) - Инверсия зависимостей для внешних систем
    /// Все внешние зависимости (MongoDB, Redis, RabbitMQ) инкапсулированы
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Настройки конфигурации
            services.Configure<ConnectionStringsSettings>(
                configuration.GetSection("ConnectionStrings"));
            // Use correct section name from appsettings.json
            services.Configure<DatabaseSettings>(
                configuration.GetSection("DatabaseSettings"));
            services.Configure<CacheSettings>(
                configuration.GetSection("CacheSettings"));

            // ==========================================
            // MongoDB - подключаемся к существующей базе
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
                // Read database name from correct configuration section
                var databaseName = configuration.GetValue<string>("DatabaseSettings:DatabaseName");
                if (string.IsNullOrWhiteSpace(databaseName))
                {
                    throw new InvalidOperationException("MongoDB database name is not configured. Please set 'DatabaseSettings:DatabaseName' in configuration.");
                }

                return client.GetDatabase(databaseName);
            });

            // Доменная зависимость - DIP
            services.AddSingleton<IMongoDbConnection, MongoDbConnection>();

            // ==========================================
            // Redis - подключаемся к существующему экземпляру
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

            // Доменная зависимость - DIP
            services.AddSingleton<IRedisConnection, RedisConnection>();

            // ==========================================
            // RabbitMQ - подключаемся к существующему брокеру
            // ==========================================

            services.AddSingleton<IConnection>(sp =>
            {
                var connectionString = configuration.GetConnectionString("RabbitMQ");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("RabbitMQ connection string is not configured. Please set 'ConnectionStrings:RabbitMQ' in configuration.");
                }

                var factory = new ConnectionFactory
                {
                    Uri = new Uri(connectionString),
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                    RequestedHeartbeat = TimeSpan.FromSeconds(60)
                };

                // Создаем подключение к уже развернутому брокеру
                return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            });

            // Доменная зависимость - DIP
            services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();

            // ==========================================
            // Domain Services для инфраструктуры - DIP
            // ==========================================

            services.AddSingleton<ICacheService, RedisCacheService>();
            services.AddSingleton<IMessageBus, RabbitMqMessageBus>();

            // ==========================================
            // НОВОЕ: Auth Services - JWT и Password Hashing
            // ==========================================
            
            services.AddSingleton<IJwtTokenService, JwtTokenService>();
            services.AddSingleton<IPasswordHashService, PasswordHashService>();
            
            // НОВОЕ: Auth Cache Service - кеширование через Redis
            services.AddSingleton<IAuthCacheService, AuthCacheService>();

            // ==========================================
            // Repositories - DIP
            // ==========================================

            // Register MongoDB repository that implements IPageContentRepository
            services.AddScoped<IPageContentRepository, PageContentRepository>(sp =>
            {
                var database = sp.GetRequiredService<IMongoDatabase>();
                var logger = sp.GetRequiredService<ILogger<PageContentRepository>>();
                // use collection name 'PageContent' to match other parts of code
                return new PageContentRepository(database, "PageContent", logger);
            });

            // НОВОЕ: User Repository
            services.AddScoped<IUserRepository, UserRepository>(sp =>
            {
                var database = sp.GetRequiredService<IMongoDatabase>();
                var logger = sp.GetRequiredService<ILogger<UserRepository>>();
                return new UserRepository(database, logger);
            });

            // НОВОЕ: RefreshToken Repository - хранение в MongoDB
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>(sp =>
            {
                var database = sp.GetRequiredService<IMongoDatabase>();
                var logger = sp.GetRequiredService<ILogger<RefreshTokenRepository>>();
                return new RefreshTokenRepository(database, logger);
            });

            // Domain Services
            services.AddScoped<IPageContentService, PageContentService>();

            return services;
        }

        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Application Services
            services.AddScoped<IPageContentApplicationService, PageContentApplicationService>();
            
            // НОВОЕ: Auth Application Service
            services.AddScoped<IAuthApplicationService, AuthApplicationService>();

            return services;
        }

        // AddDomain extension to satisfy Program.cs call. No-op or register domain-specific services here.
        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            // Domain layer typically contains interfaces and domain services. Register domain-level services if needed.
            return services;
        }
    }
}