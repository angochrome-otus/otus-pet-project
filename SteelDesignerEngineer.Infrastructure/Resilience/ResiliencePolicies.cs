using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Microsoft.Extensions.Logging;

namespace SteelDesignerEngineer.Infrastructure.Resilience;

/// <summary>
/// Resilience policies for microservices communication
/// Implements Circuit Breaker, Retry, and Timeout patterns
/// </summary>
public static class ResiliencePolicies
{
    /// <summary>
    /// Retry policy with exponential backoff
    /// Retries 3 times: wait 1s, 2s, 4s
    /// </summary>
    public static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    var logger = context.GetLogger();
                    logger?.LogWarning(
                        "Retry {RetryCount} after {Delay}s due to {Reason}",
                        retryCount,
                        timespan.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()
                    );
                }
            );
    }

    /// <summary>
    /// Circuit breaker policy
    /// Opens after 5 consecutive failures, stays open for 30s
    /// </summary>
    public static AsyncCircuitBreakerPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .Or<TimeoutException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, duration, context) =>
                {
                    var logger = context.GetLogger();
                    logger?.LogError(
                        "Circuit breaker opened for {Duration}s due to {Reason}",
                        duration.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()
                    );
                },
                onReset: context =>
                {
                    var logger = context.GetLogger();
                    logger?.LogInformation("Circuit breaker reset");
                },
                onHalfOpen: () =>
                {
                    // Log when circuit breaker is testing
                }
            );
    }

    /// <summary>
    /// Timeout policy - 30 seconds per request
    /// </summary>
    public static AsyncTimeoutPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {
        return Policy
            .TimeoutAsync<HttpResponseMessage>(
                timeout: TimeSpan.FromSeconds(30),
                onTimeoutAsync: (context, timespan, task) =>
                {
                    var logger = context.GetLogger();
                    logger?.LogWarning("Request timeout after {Timeout}s", timespan.TotalSeconds);
                    return Task.CompletedTask;
                }
            );
    }

    /// <summary>
    /// Combined policy: Timeout ? Retry ? Circuit Breaker
    /// Order matters: innermost policy executes first
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
    {
        return Policy.WrapAsync(
            GetCircuitBreakerPolicy(),
            GetRetryPolicy(),
            GetTimeoutPolicy()
        );
    }

    /// <summary>
    /// RabbitMQ retry policy for message processing
    /// </summary>
    public static AsyncRetryPolicy GetRabbitMqRetryPolicy()
    {
        return Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)),
                onRetry: (exception, timespan, retryCount, context) =>
                {
                    var logger = context.GetLogger();
                    logger?.LogWarning(
                        exception,
                        "RabbitMQ retry {RetryCount} after {Delay}s",
                        retryCount,
                        timespan.TotalSeconds
                    );
                }
            );
    }

    /// <summary>
    /// Circuit breaker for RabbitMQ
    /// </summary>
    public static AsyncCircuitBreakerPolicy GetRabbitMqCircuitBreakerPolicy()
    {
        return Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration, context) =>
                {
                    var logger = context.GetLogger();
                    logger?.LogError(
                        exception,
                        "RabbitMQ circuit breaker opened for {Duration}s",
                        duration.TotalSeconds
                    );
                },
                onReset: context =>
                {
                    var logger = context.GetLogger();
                    logger?.LogInformation("RabbitMQ circuit breaker reset");
                }
            );
    }

    private static ILogger? GetLogger(this Context context)
    {
        if (context.TryGetValue("Logger", out var logger))
        {
            return logger as ILogger;
        }
        return null;
    }
}
