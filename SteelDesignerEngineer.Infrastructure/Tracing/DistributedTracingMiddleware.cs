using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;

namespace SteelDesignerEngineer.Infrastructure.Tracing;

/// <summary>
/// Middleware for distributed tracing using Activity (W3C Trace Context)
/// Adds correlation ID to all logs and HTTP headers
/// </summary>
public class DistributedTracingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DistributedTracingMiddleware> _logger;
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private const string TraceIdHeader = "X-Trace-ID";

    public DistributedTracingMiddleware(
        RequestDelegate next,
        ILogger<DistributedTracingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var activity = Activity.Current;
        
        // Get or generate correlation ID
        var correlationId = GetOrGenerateCorrelationId(context);
        
        // Add to response headers for client tracking
        context.Response.Headers[CorrelationIdHeader] = correlationId;
        
        if (activity != null)
        {
            // Add trace ID to response
            context.Response.Headers[TraceIdHeader] = activity.TraceId.ToString();
            
            // Add correlation ID as tag to activity
            activity.AddTag("correlation_id", correlationId);
            activity.AddTag("http.method", context.Request.Method);
            activity.AddTag("http.path", context.Request.Path);
            activity.AddTag("http.host", context.Request.Host.ToString());
            
            // Add user info if authenticated
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                activity.AddTag("user.id", context.User.FindFirst("sub")?.Value ?? "unknown");
            }
        }

        // Add to HttpContext.Items for access in controllers
        context.Items["CorrelationId"] = correlationId;
        context.Items["TraceId"] = activity?.TraceId.ToString() ?? "none";

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["TraceId"] = activity?.TraceId.ToString() ?? "none",
            ["SpanId"] = activity?.SpanId.ToString() ?? "none"
        }))
        {
            _logger.LogInformation(
                "Request started: {Method} {Path}",
                context.Request.Method,
                context.Request.Path
            );

            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                
                _logger.LogInformation(
                    "Request completed: {Method} {Path} {StatusCode} in {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds
                );
            }
        }
    }

    private static string GetOrGenerateCorrelationId(HttpContext context)
    {
        // Check if client sent correlation ID
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId) 
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        // Generate new correlation ID
        return Guid.NewGuid().ToString("N");
    }
}

/// <summary>
/// Extension methods for distributed tracing
/// </summary>
public static class DistributedTracingExtensions
{
    public static IApplicationBuilder UseDistributedTracing(this IApplicationBuilder app)
    {
        return app.UseMiddleware<DistributedTracingMiddleware>();
    }

    public static string? GetCorrelationId(this HttpContext context)
    {
        return context.Items["CorrelationId"] as string;
    }

    public static string? GetTraceId(this HttpContext context)
    {
        return context.Items["TraceId"] as string;
    }
}
