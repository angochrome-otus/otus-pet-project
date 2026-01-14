# Multi-stage build for .NET 10 WebAPI application
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

# Copy solution file
COPY OTUS.Pet.Education/*.sln ./OTUS.Pet.Education/

# Copy only WebAPI and library projects (exclude WPF desktop app)
COPY OTUS.Pet.Education/SteelDesignerEngineer/*.csproj ./OTUS.Pet.Education/SteelDesignerEngineer/
COPY OTUS.Pet.Education/SteelDesignerEngineer.Application/*.csproj ./OTUS.Pet.Education/SteelDesignerEngineer.Application/
COPY OTUS.Pet.Education/SteelDesignerEngineer.Data/*.csproj ./OTUS.Pet.Education/SteelDesignerEngineer.Data/
COPY OTUS.Pet.Education/SteelDesignerEngineer.Domain/*.csproj ./OTUS.Pet.Education/SteelDesignerEngineer.Domain/
COPY OTUS.Pet.Education/SteelDesignerEngineer.Infrastructure/*.csproj ./OTUS.Pet.Education/SteelDesignerEngineer.Infrastructure/

# Restore dependencies (only for WebAPI project)
RUN dotnet restore OTUS.Pet.Education/SteelDesignerEngineer/SteelDesignerEngineer.csproj

# Copy source code (WPF excluded via .dockerignore)
COPY OTUS.Pet.Education/SteelDesignerEngineer/ ./OTUS.Pet.Education/SteelDesignerEngineer/
COPY OTUS.Pet.Education/SteelDesignerEngineer.Application/ ./OTUS.Pet.Education/SteelDesignerEngineer.Application/
COPY OTUS.Pet.Education/SteelDesignerEngineer.Data/ ./OTUS.Pet.Education/SteelDesignerEngineer.Data/
COPY OTUS.Pet.Education/SteelDesignerEngineer.Domain/ ./OTUS.Pet.Education/SteelDesignerEngineer.Domain/
COPY OTUS.Pet.Education/SteelDesignerEngineer.Infrastructure/ ./OTUS.Pet.Education/SteelDesignerEngineer.Infrastructure/

# Build and publish WebAPI project
RUN dotnet publish OTUS.Pet.Education/SteelDesignerEngineer/SteelDesignerEngineer.csproj \
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