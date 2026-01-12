using SteelDesignerEngineer.Infrastructure.DependencyInjection;
using SteelDesignerEngineer.WebApi.BackgroundServices;
using SteelDesignerEngineer.WebApi.MessageHandlers;
using Serilog;//Логирование
using Microsoft.OpenApi.Models;//API
using Microsoft.Extensions.DependencyInjection;
using SteelDesignerEngineer.Domain.Entities;
using System.Text.Json;
using System.IO;
using SteelDesignerEngineer.Application.Interfaces;
using SteelDesignerEngineer.Domain.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Настройка Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .WriteTo.Console()
            .WriteTo.File("logs/api-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();
        
        // ИСПРАВЛЕНО: Не переопределяем URLs - используем значение из переменной окружения ASPNETCORE_URLS
        // В Production (Kubernetes) будет использоваться http://+:80
        // В Development можно задать через launchSettings.json
        // builder.WebHost.UseUrls("https://localhost:65030", "http://localhost:65031");

        // Регистрация слоев Clean Architecture
        builder.Services.AddDomain();
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);

        // ==========================================
        // НОВОЕ: JWT Authentication
        // ==========================================
        
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];
            if (string.IsNullOrEmpty(jwtSecretKey))
            {
                Log.Warning("JWT SecretKey not configured. JWT authentication will not work.");
                jwtSecretKey = "DEFAULT_KEY_FOR_DEVELOPMENT_ONLY_MINIMUM_32_CHARACTERS_LONG_123456";
            }

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "SteelDesignerEngineer",
                ValidAudience = builder.Configuration["Jwt:Audience"] ?? "SteelDesignerEngineer.Client",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        builder.Services.AddAuthorization();

        // Добавление поддержки REST API
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        
        // Configure Swagger document с поддержкой JWT
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "SteelDesignerEngineer API", 
                Version = "v1",
                Description = "API для образовательного портала по проектированию стальных конструкций"
            });

            // НОВОЕ: Добавляем поддержку JWT в Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // Настройка CORS для доступа с фронтенда
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Регистрация обработчиков сообщений RabbitMQ
        builder.Services.AddSingleton<HealthMessageHandler>();

        // Регистрация Background Service для обработки сообщений RabbitMQ
        builder.Services.AddHostedService<MessageHandlerHostedService>();

        var app = builder.Build();

        // Всегда включаем Swagger UI
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Steel Designer Engineer API v1");
            c.RoutePrefix = "swagger";
        });

        // Поддержка статических файлов (HTML интерфейс)
        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseCors("AllowAll");
        
        // НОВОЕ: JWT Authentication & Authorization middleware
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapControllers();

        // Получаем URLs из конфигурации
        var urls = builder.Configuration["ASPNETCORE_URLS"] ?? app.Urls.FirstOrDefault() ?? "http://localhost:80";
        
        Log.Information("SteelDesignerEngineer API starting...");
        Log.Information("Listening on: {Urls}", urls);
        Log.Information("Swagger UI доступен на: {Urls}/swagger", urls.Split(';')[0]);

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        // Seed default page into storage and messaging on startup
        string? seededPageName = null;
        try
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            var config = services.GetRequiredService<IConfiguration>();
            var defaultPageName = config.GetValue<string>("SiteSettings:DefaultPage", "home");
            seededPageName = defaultPageName;

            var pageContentService = services.GetRequiredService<IPageContentApplicationService>();
            var messageBus = services.GetRequiredService<IMessageBus>();
            var cacheService = services.GetRequiredService<ICacheService>();
            var logger = services.GetRequiredService<ILogger<Program>>();

            // Read HTML from wwwroot/index.html if exists
            var indexPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "index.html");
            var html = File.Exists(indexPath) ? File.ReadAllText(indexPath) : "<h1>Welcome</h1>";

            var page = new PageContent
            {
                PageName = defaultPageName,
                Title = "Главная",
                Content = html,
                LastModified = DateTime.UtcNow
            };

            // ИСПРАВЛЕНО: Проверяем существование страницы перед созданием
            var existingPage = pageContentService.GetPageContentByPageNameAsync(defaultPageName).GetAwaiter().GetResult();
            
            if (existingPage == null)
            {
                // Create page in MongoDB only if it doesn't exist
                var created = pageContentService.CreatePageContentAsync(page).GetAwaiter().GetResult();

                // Cache in Redis
                try
                {
                    var key = $"page:{created.PageName}";
                    var json = JsonSerializer.Serialize(created);
                    cacheService.SetAsync(key, json).GetAwaiter().GetResult();
                    logger.LogInformation("Cached default page in Redis: {Key}", key);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to cache default page in Redis");
                }

                // Publish event to RabbitMQ
                try
                {
                    messageBus.PublishAsync("page_content_created", created).GetAwaiter().GetResult();
                    logger.LogInformation("Published page_content_created for page: {PageName}", created.PageName);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to publish default page creation event");
                }
            }
            else
            {
                logger.LogInformation("Default page '{PageName}' already exists, skipping seeding", defaultPageName);
            }
        }
        catch (Exception ex)
        {
            var logger = app.Services.GetService<ILogger<Program>>();
            logger?.LogError(ex, "Error seeding default page on startup");
            // НЕ падаем при ошибке seeding - продолжаем запуск приложения
        }

        // Log direct links to the site and default page API
        try
        {
            var baseUrl = urls.Split(';')[0];
            var pageName = seededPageName ?? "home";

            Log.Information("Main site: {Url}", baseUrl);
            Log.Information("Default page API: {Url}/api/PageContent/by-name/{PageName}", baseUrl, pageName);
            Log.Information("Auth API: {Url}/api/Auth/login", baseUrl);
        }
        catch { }

        app.Run();
        Log.Information("SteelDesignerEngineer API stopped");
        Log.CloseAndFlush();
    }
}