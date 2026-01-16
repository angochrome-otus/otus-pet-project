FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all project files with correct dependencies
COPY ["SteelDesignerEngineer.ApiGateway/SteelDesignerEngineer.ApiGateway.csproj", "SteelDesignerEngineer.ApiGateway/"]
COPY ["SteelDesignerEngineer.Contracts/SteelDesignerEngineer.Contracts.csproj", "SteelDesignerEngineer.Contracts/"]
COPY ["SteelDesignerEngineer.Infrastructure/SteelDesignerEngineer.Infrastructure.csproj", "SteelDesignerEngineer.Infrastructure/"]
COPY ["SteelDesignerEngineer.Application/SteelDesignerEngineer.Application.csproj", "SteelDesignerEngineer.Application/"]
COPY ["SteelDesignerEngineer.Domain/SteelDesignerEngineer.Domain.csproj", "SteelDesignerEngineer.Domain/"]
COPY ["SteelDesignerEngineer.Data/SteelDesignerEngineer.Data.csproj", "SteelDesignerEngineer.Data/"]

# Restore dependencies
RUN dotnet restore "SteelDesignerEngineer.ApiGateway/SteelDesignerEngineer.ApiGateway.csproj"

# Copy all source files
COPY . .

# Build
WORKDIR "/src/SteelDesignerEngineer.ApiGateway"
RUN dotnet build "SteelDesignerEngineer.ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SteelDesignerEngineer.ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "SteelDesignerEngineer.ApiGateway.dll"]
