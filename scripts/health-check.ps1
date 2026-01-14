# Health Check Script for Steel Designer Engineer (PowerShell)
# Usage: .\health-check.ps1 [host] [port]

param(
    [string]$Host = "localhost",
    [int]$Port = 5000
)

$BaseUrl = "http://${Host}:${Port}"

Write-Host "?? Starting health check for Steel Designer Engineer..." -ForegroundColor Cyan
Write-Host "?? Target: $BaseUrl" -ForegroundColor Cyan
Write-Host ""

function Test-Endpoint {
    param(
        [string]$Endpoint,
        [string]$Description
    )
    
    Write-Host "Checking $Description... " -NoNewline
    
    try {
        $response = Invoke-WebRequest -Uri "$BaseUrl$Endpoint" -UseBasicParsing -TimeoutSec 10 -ErrorAction Stop
        
        if ($response.StatusCode -eq 200 -or $response.StatusCode -eq 302) {
            Write-Host "? OK" -ForegroundColor Green -NoNewline
            Write-Host " (HTTP $($response.StatusCode))" -ForegroundColor Gray
            return $true
        }
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "? FAILED" -ForegroundColor Red -NoNewline
        Write-Host " (HTTP $statusCode)" -ForegroundColor Gray
        return $false
    }
}

function Test-DockerContainer {
    param(
        [string]$ContainerName,
        [string]$Description
    )
    
    Write-Host "Checking $Description... " -NoNewline
    
    try {
        $container = docker ps --filter "name=$ContainerName" --format "{{.Status}}" 2>$null
        
        if ($container -and $container -like "*Up*") {
            Write-Host "? Running" -ForegroundColor Green
            return $true
        }
        else {
            Write-Host "? Not running" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "? Docker not available" -ForegroundColor Red
        return $false
    }
}

$failed = 0

# Check Docker environment if available
if (Get-Command docker -ErrorAction SilentlyContinue) {
    Write-Host "?? Docker Environment Detected" -ForegroundColor Cyan
    Write-Host "================================"
    
    if (-not (Test-DockerContainer "steel-designer-engineer-api" "WebAPI Container")) { $failed++ }
    if (-not (Test-DockerContainer "steel-designer-mongodb" "MongoDB Container")) { $failed++ }
    if (-not (Test-DockerContainer "steel-designer-redis" "Redis Container")) { $failed++ }
    if (-not (Test-DockerContainer "steel-designer-rabbitmq" "RabbitMQ Container")) { $failed++ }
    
    Write-Host ""
}

# Check WebAPI endpoints
Write-Host "?? Checking WebAPI Endpoints" -ForegroundColor Cyan
Write-Host "================================"

if (-not (Test-Endpoint "/swagger/index.html" "Swagger UI")) { $failed++ }
if (-not (Test-Endpoint "/index.html" "Main Page")) { $failed++ }
if (-not (Test-Endpoint "/login.html" "Login Page")) { $failed++ }

Write-Host ""

# Summary
Write-Host "?? Health Check Summary" -ForegroundColor Cyan
Write-Host "================================"

if ($failed -eq 0) {
    Write-Host "? All checks passed!" -ForegroundColor Green
    exit 0
}
else {
    Write-Host "? $failed check(s) failed" -ForegroundColor Red
    exit 1
}
