FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all project files with correct paths
COPY ["SteelDesignerEngineer.Services.Auth/SteelDesignerEngineer.Services.Auth.csproj", "SteelDesignerEngineer.Services.Auth/"]
COPY ["SteelDesignerEngineer.Contracts/SteelDesignerEngineer.Contracts.csproj", "SteelDesignerEngineer.Contracts/"]
COPY ["SteelDesignerEngineer.Infrastructure/SteelDesignerEngineer.Infrastructure.csproj", "SteelDesignerEngineer.Infrastructure/"]
COPY ["SteelDesignerEngineer.Application/SteelDesignerEngineer.Application.csproj", "SteelDesignerEngineer.Application/"]
COPY ["SteelDesignerEngineer.Domain/SteelDesignerEngineer.Domain.csproj", "SteelDesignerEngineer.Domain/"]
COPY ["SteelDesignerEngineer.Data/SteelDesignerEngineer.Data.csproj", "SteelDesignerEngineer.Data/"]

# Restore dependencies
RUN dotnet restore "SteelDesignerEngineer.Services.Auth/SteelDesignerEngineer.Services.Auth.csproj"

# Copy all source files
COPY . .

# Build
WORKDIR "/src/SteelDesignerEngineer.Services.Auth"
RUN dotnet build "SteelDesignerEngineer.Services.Auth.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SteelDesignerEngineer.Services.Auth.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "SteelDesignerEngineer.Services.Auth.dll"]
