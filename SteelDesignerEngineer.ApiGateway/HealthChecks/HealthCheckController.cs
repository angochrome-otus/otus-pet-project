using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SteelDesignerEngineer.ApiGateway.HealthChecks;

/// <summary>
/// Health check endpoints for Kubernetes probes
/// </summary>
[ApiController]
[Route("health")]
public class HealthCheckController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthCheckController> _logger;

    public HealthCheckController(
        HealthCheckService healthCheckService,
        ILogger<HealthCheckController> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    /// <summary>
    /// Liveness probe - checks if application is running
    /// Used by Kubernetes to restart unhealthy pods
    /// </summary>
    [HttpGet("live")]
    [ProducesResponseType(200)]
    [ProducesResponseType(503)]
    public async Task<IActionResult> LivenessProbe()
    {
        try
        {
            // Simple check - if we can respond, we're alive
            return Ok(new
            {
                status = "Healthy",
                service = "API Gateway",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Liveness probe failed");
            return StatusCode(503, new
            {
                status = "Unhealthy",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Readiness probe - checks if application is ready to serve traffic
    /// Checks all dependencies (MongoDB, Redis, RabbitMQ)
    /// Used by Kubernetes to route traffic only to ready pods
    /// </summary>
    [HttpGet("ready")]
    [ProducesResponseType(200)]
    [ProducesResponseType(503)]
    public async Task<IActionResult> ReadinessProbe()
    {
        try
        {
            var healthReport = await _healthCheckService.CheckHealthAsync();

            if (healthReport.Status == HealthStatus.Healthy)
            {
                return Ok(new
                {
                    status = "Healthy",
                    service = "API Gateway",
                    checks = healthReport.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        duration = e.Value.Duration.TotalMilliseconds
                    }),
                    timestamp = DateTime.UtcNow
                });
            }

            _logger.LogWarning("Readiness probe failed: {Status}", healthReport.Status);
            return StatusCode(503, new
            {
                status = healthReport.Status.ToString(),
                checks = healthReport.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    error = e.Value.Exception?.Message,
                    duration = e.Value.Duration.TotalMilliseconds
                }),
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness probe failed with exception");
            return StatusCode(503, new
            {
                status = "Unhealthy",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Startup probe - checks if application has completed initialization
    /// Used by Kubernetes for slow-starting applications
    /// </summary>
    [HttpGet("startup")]
    [ProducesResponseType(200)]
    [ProducesResponseType(503)]
    public async Task<IActionResult> StartupProbe()
    {
        try
        {
            var healthReport = await _healthCheckService.CheckHealthAsync();

            if (healthReport.Status == HealthStatus.Healthy)
            {
                return Ok(new
                {
                    status = "Started",
                    service = "API Gateway",
                    timestamp = DateTime.UtcNow
                });
            }

            return StatusCode(503, new
            {
                status = "Starting",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Startup probe failed");
            return StatusCode(503, new
            {
                status = "Failed",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
