# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5004

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["SteelDesignerEngineer.Services.Search/SteelDesignerEngineer.Services.Search.csproj", "SteelDesignerEngineer.Services.Search/"]
COPY ["SteelDesignerEngineer.Contracts/SteelDesignerEngineer.Contracts.csproj", "SteelDesignerEngineer.Contracts/"]
COPY ["SteelDesignerEngineer.Infrastructure/SteelDesignerEngineer.Infrastructure.csproj", "SteelDesignerEngineer.Infrastructure/"]
COPY ["SteelDesignerEngineer.Application/SteelDesignerEngineer.Application.csproj", "SteelDesignerEngineer.Application/"]
COPY ["SteelDesignerEngineer.Domain/SteelDesignerEngineer.Domain.csproj", "SteelDesignerEngineer.Domain/"]
COPY ["SteelDesignerEngineer.Data/SteelDesignerEngineer.Data.csproj", "SteelDesignerEngineer.Data/"]

RUN dotnet restore "SteelDesignerEngineer.Services.Search/SteelDesignerEngineer.Services.Search.csproj"

COPY . .
WORKDIR "/src/SteelDesignerEngineer.Services.Search"
RUN dotnet build "SteelDesignerEngineer.Services.Search.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SteelDesignerEngineer.Services.Search.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "SteelDesignerEngineer.Services.Search.dll"]
