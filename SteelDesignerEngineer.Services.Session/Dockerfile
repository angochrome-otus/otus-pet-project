FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all project files
COPY ["SteelDesignerEngineer.Services.Session/SteelDesignerEngineer.Services.Session.csproj", "SteelDesignerEngineer.Services.Session/"]
COPY ["SteelDesignerEngineer.Contracts/SteelDesignerEngineer.Contracts.csproj", "SteelDesignerEngineer.Contracts/"]
COPY ["SteelDesignerEngineer.Infrastructure/SteelDesignerEngineer.Infrastructure.csproj", "SteelDesignerEngineer.Infrastructure/"]
COPY ["SteelDesignerEngineer.Application/SteelDesignerEngineer.Application.csproj", "SteelDesignerEngineer.Application/"]
COPY ["SteelDesignerEngineer.Domain/SteelDesignerEngineer.Domain.csproj", "SteelDesignerEngineer.Domain/"]
COPY ["SteelDesignerEngineer.Data/SteelDesignerEngineer.Data.csproj", "SteelDesignerEngineer.Data/"]

# Restore
RUN dotnet restore "SteelDesignerEngineer.Services.Session/SteelDesignerEngineer.Services.Session.csproj"

# Copy source
COPY . .

# Build
WORKDIR "/src/SteelDesignerEngineer.Services.Session"
RUN dotnet build "SteelDesignerEngineer.Services.Session.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SteelDesignerEngineer.Services.Session.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "SteelDesignerEngineer.Services.Session.dll"]
