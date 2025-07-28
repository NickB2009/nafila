# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80
EXPOSE 443


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GrandeTech.QueueHub.API/GrandeTech.QueueHub.API.csproj", "GrandeTech.QueueHub.API/"]
COPY ["GrandeTech.QueueHub.Tests/GrandeTech.QueueHub.Tests.csproj", "GrandeTech.QueueHub.Tests/"]
RUN dotnet restore "GrandeTech.QueueHub.API/GrandeTech.QueueHub.API.csproj"
COPY . .
WORKDIR "/src/GrandeTech.QueueHub.API"
RUN dotnet build "GrandeTech.QueueHub.API.csproj" -c Release -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "GrandeTech.QueueHub.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set production environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80;https://+:443

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "GrandeTech.QueueHub.API.dll"]