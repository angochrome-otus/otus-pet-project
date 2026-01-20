using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using SteelDesignerEngineer.ApiGateway.Clients;
using SteelDesignerEngineer.ApiGateway.Messaging;
using SteelDesignerEngineer.ApiGateway.Session;
using Serilog;
using RabbitMQ.Client;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/gateway-.log", rollingInterval: RollingInterval.Day)
    .Enrich.WithProperty("ServiceName", "API-Gateway")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddRazorPages();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value" : e.ErrorMessage).ToArray()
            );

        return new BadRequestObjectResult(new
        {
            success = false,
            message = "Validation failed",
            errors
        });
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Steel Designer Engineer API",
        Version = "v1",
        Description = "Steel Designer Engineer API Gateway"
    });
});

builder.Services.AddHealthChecks();

builder.Services.AddOpenTelemetry()
    .WithTracing(tp =>
    {
        tp.SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("SteelDesignerEngineer.ApiGateway")
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = builder.Environment.EnvironmentName,
                    ["service.version"] = "1.0.0"
                }))
            .AddAspNetCoreInstrumentation(o =>
            {
                o.RecordException = true;
                o.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health") && !ctx.Request.Path.StartsWithSegments("/metrics");
            })
            .AddHttpClientInstrumentation()
            .AddSource("SteelDesignerEngineer.*")
            .AddConsoleExporter();
    });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    // Secure should be enabled only when request is HTTPS (or behind proxy with forwarded proto)
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".SteelDesignerEngineer.Session";
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5000",
                "https://localhost:5001",
                "https://steel-designer-engineer.ru",
                "http://steel-designer-engineer.ru")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddSingleton<IConnection>(_ =>
{
    var cs = builder.Configuration.GetConnectionString("RabbitMQ")
             ?? throw new InvalidOperationException("ConnectionStrings:RabbitMQ is required");
    return RabbitMqConnectionFactory.Create(cs);
});

builder.Services.AddSingleton<RabbitMqRpcClient>();

builder.Services.AddScoped<IAuthServiceClient, AuthServiceClient>();
builder.Services.AddScoped<ISessionServiceClient, SessionServiceClient>();
builder.Services.AddScoped<IPageContentServiceClient, PageContentServiceClient>();
builder.Services.AddScoped<ISearchServiceClient, SearchServiceClient>();

// Forwarded headers so IsHttps works behind ingress
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseMetricServer();
app.UseHttpMetrics();

app.UseIpRateLimiting();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Steel Designer Engineer API v1");
    c.RoutePrefix = "swagger";
});

// Restrict Swagger to authenticated admins only
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
    {
        if (!context.IsSessionAuthenticated())
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Not authenticated" });
            return;
        }

        var role = (context.Items.TryGetValue("UserRole", out var r) ? r as string : null) ?? string.Empty;
        if (!string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { message = "Forbidden" });
            return;
        }
    }

    await next();
});

app.UseForwardedHeaders();

// Enable HTTPS redirection only when HTTPS is actually configured
var urls = builder.Configuration["ASPNETCORE_URLS"];
var httpsEnabled = (!string.IsNullOrWhiteSpace(urls) && urls.Contains("https", StringComparison.OrdinalIgnoreCase))
                   || builder.Configuration.GetValue<bool>("EnableHttpsRedirection");
if (httpsEnabled)
{
    app.UseHttpsRedirection();
}

// Serve wwwroot assets (favicon, etc.)
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");
app.UseSession();

app.UseMiddleware<SessionCookieMiddleware>();

// Ensure HttpContext.User etc. are populated before Authorization (even if you later add auth)
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { Predicate = _ => true });

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllers();
app.MapRazorPages();

app.Run();
