# Multi-stage build for .NET 10 WebAPI application
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

# Copy solution file
COPY *.sln ./

# Copy only WebAPI and library projects (exclude WPF desktop app)
COPY SteelDesignerEngineer/*.csproj ./SteelDesignerEngineer/
COPY SteelDesignerEngineer.Application/*.csproj ./SteelDesignerEngineer.Application/
COPY SteelDesignerEngineer.Data/*.csproj ./SteelDesignerEngineer.Data/
COPY SteelDesignerEngineer.Domain/*.csproj ./SteelDesignerEngineer.Domain/
COPY SteelDesignerEngineer.Infrastructure/*.csproj ./SteelDesignerEngineer.Infrastructure/

# Restore dependencies (only for WebAPI project)
RUN dotnet restore SteelDesignerEngineer/SteelDesignerEngineer.csproj

# Copy source code (WPF excluded via .dockerignore)
COPY SteelDesignerEngineer/ ./SteelDesignerEngineer/
COPY SteelDesignerEngineer.Application/ ./SteelDesignerEngineer.Application/
COPY SteelDesignerEngineer.Data/ ./SteelDesignerEngineer.Data/
COPY SteelDesignerEngineer.Domain/ ./SteelDesignerEngineer.Domain/
COPY SteelDesignerEngineer.Infrastructure/ ./SteelDesignerEngineer.Infrastructure/

# Build and publish WebAPI project
RUN dotnet publish SteelDesignerEngineer/SteelDesignerEngineer.csproj \
    -c Release \
    -o out \
    --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy published output
COPY --from=build-env /app/out .

# Environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Expose HTTP port
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
  CMD curl --fail http://localhost:80/swagger/index.html || exit 1

# Run the application
ENTRYPOINT ["dotnet", "SteelDesignerEngineer.dll"]