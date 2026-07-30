# Build context is the repository root (see docker-compose.yml "context: ..").
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY nuget.config .
COPY nugets/ nugets/
COPY src/FileManager.Api/FileManager.Api.csproj src/FileManager.Api/
COPY src/FileManager.Infrastructure/FileManager.Infrastructure.csproj src/FileManager.Infrastructure/
COPY src/FileManager.Application/FileManager.Application.csproj src/FileManager.Application/
COPY src/FileManager.Domain/FileManager.Domain.csproj src/FileManager.Domain/
COPY src/FileManager.Contracts/FileManager.Contracts.csproj src/FileManager.Contracts/
RUN dotnet restore src/FileManager.Api/FileManager.Api.csproj

COPY src/ src/
WORKDIR /src/src/FileManager.Api
RUN dotnet build FileManager.Api.csproj -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish FileManager.Api.csproj -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileManager.Api.dll"]
