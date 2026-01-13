using SteelDesignerEngineer.Infrastructure.DependencyInjection;
using SteelDesignerEngineer.WebApi.BackgroundServices;
using SteelDesignerEngineer.WebApi.MessageHandlers;
using Serilog;
using Microsoft.OpenApi.Models;
using SteelDesignerEngineer.Domain.Entities;
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
        
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .WriteTo.Console()
            .WriteTo.File("logs/api-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();

        // Clean Architecture: Layer registration
        builder.Services.AddDomain();
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);

        // Session support
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromHours(24);
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.IsEssential = true;
        });

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "SteelDesignerEngineer API", 
                Version = "v1",
                Description = "API for Steel Designer Engineer - Session-based authentication"
            });
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.WithOrigins(
                        "http://localhost:5000", 
                        "https://localhost:5001",
                        "https://steel-designer-engineer.ru",
                        "http://steel-designer-engineer.ru"
                      )
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials(); // Important for cookies
            });
        });

        builder.Services.AddSingleton<HealthMessageHandler>();
        builder.Services.AddHostedService<MessageHandlerHostedService>();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Steel Designer Engineer API v1");
            c.RoutePrefix = "swagger";
        });

        // Set UTF-8 charset for HTML files
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.Value?.EndsWith(".html") == true)
            {
                context.Response.Headers["Content-Type"] = "text/html; charset=utf-8";
            }
            await next();
        });

        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseCors("AllowAll");
        
        // Session middleware (ASP.NET built-in for OAuth CSRF)
        app.UseSession();
        
        // Custom Session Cookie Middleware (validates Redis sessions)
        app.UseMiddleware<SteelDesignerEngineer.Infrastructure.Middleware.SessionCookieMiddleware>();
        
        app.MapControllers();

        var urls = builder.Configuration["ASPNETCORE_URLS"] ?? app.Urls.FirstOrDefault() ?? "http://localhost:80";
        
        Log.Information("=".PadRight(60, '='));
        Log.Information("SteelDesignerEngineer API starting...");
        Log.Information("Listening on: {Urls}", urls);
        Log.Information("Swagger UI: {Urls}/swagger", urls.Split(';')[0]);
        Log.Information("=".PadRight(60, '='));

        app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

        // SEEDING
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();
            var wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");

            // Seed all HTML pages from wwwroot to MongoDB
            try
            {
                Log.Information("📄 Seeding all HTML pages to MongoDB...");
                SteelDesignerEngineer.WebApi.Seeders.PagesSeeder.SeedAllPagesAsync(services, logger, wwwrootPath).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Critical error seeding pages");
            }

            // Seed admin user
            try
            {
                Log.Information("👤 Seeding admin user...");
                SteelDesignerEngineer.WebApi.Seeders.AdminSeeder.SeedAdminUserAsync(services, logger).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Critical error seeding admin user");
            }
        }

        Log.Information("=".PadRight(60, '='));
        Log.Information("📍 Main site: {Url}", urls.Split(';')[0]);
        Log.Information("📍 API: {Url}/api", urls.Split(';')[0]);
        Log.Information("📍 Swagger: {Url}/swagger", urls.Split(';')[0]);
        Log.Information("📍 Register: {Url}/register.html", urls.Split(';')[0]);
        Log.Information("📍 Login: {Url}/login.html", urls.Split(';')[0]);
        Log.Information("=".PadRight(60, '='));
        Log.Information("🔐 Authentication:");
        Log.Information("   - Register new account at /register.html");
        Log.Information("   - Login at /login.html");
        Log.Information("   - Session-based authentication (cookies)");
        Log.Information("   - OAuth: Google & GitHub");
        Log.Information("=".PadRight(60, '='));
        Log.Information("🔄 Session Management:");
        Log.Information("   - Sessions stored in Redis (24 hours)");
        Log.Information("   - User data cached in Redis (15 minutes)");
        Log.Information("   - Cookie-based authentication (HttpOnly, Secure)");
        Log.Information("   - Rate limiting for login attempts");
        Log.Information("=".PadRight(60, '='));
        Log.Information("📊 Session History:");
        Log.Information("   - Login/Logout tracking in MongoDB");
        Log.Information("   - IP address and User Agent logging");
        Log.Information("   - View active sessions at /api/Session/active");
        Log.Information("   - View history at /api/Session/history");
        Log.Information("=".PadRight(60, '='));

        app.Run();
        
        Log.Information("SteelDesignerEngineer API stopped");
        Log.CloseAndFlush();
    }
}