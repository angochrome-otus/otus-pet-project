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
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
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

app.UseDefaultFiles();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowAll");
app.UseSession();

app.UseMiddleware<SessionCookieMiddleware>();

app.UseAuthorization();

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { Predicate = _ => true });

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllers();
app.MapRazorPages();

app.Run();
