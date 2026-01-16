# Multi-stage build for .NET 8 WebAPI application
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy all project files (but not source code yet) for dependency resolution
COPY SteelDesignerEngineer.Domain/*.csproj ./SteelDesignerEngineer.Domain/
COPY SteelDesignerEngineer.Application/*.csproj ./SteelDesignerEngineer.Application/
COPY SteelDesignerEngineer.Data/*.csproj ./SteelDesignerEngineer.Data/
COPY SteelDesignerEngineer.Infrastructure/*.csproj ./SteelDesignerEngineer.Infrastructure/
COPY SteelDesignerEngineer/*.csproj ./SteelDesignerEngineer/

# Restore dependencies for WebAPI project (it will restore all dependencies recursively)
RUN dotnet restore SteelDesignerEngineer/SteelDesignerEngineer.csproj

# Copy all source code
COPY SteelDesignerEngineer.Domain/ ./SteelDesignerEngineer.Domain/
COPY SteelDesignerEngineer.Application/ ./SteelDesignerEngineer.Application/
COPY SteelDesignerEngineer.Data/ ./SteelDesignerEngineer.Data/
COPY SteelDesignerEngineer.Infrastructure/ ./SteelDesignerEngineer.Infrastructure/
COPY SteelDesignerEngineer/ ./SteelDesignerEngineer/

# Build and publish WebAPI project
RUN dotnet publish SteelDesignerEngineer/SteelDesignerEngineer.csproj \
    -c Release \
    -o out \
    --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published output
COPY --from=build-env /app/out .

# Environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Expose HTTP port
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=40s --retries=3 \
  CMD curl --fail http://localhost:80/swagger/index.html || exit 1

# Run the application
ENTRYPOINT ["dotnet", "SteelDesignerEngineer.dll"]