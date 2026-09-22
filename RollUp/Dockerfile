# ==============================================================================
# Multi-Stage Containerfile / Dockerfile for RollUp (.NET 10 / RHEL Podman)
# ==============================================================================

# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore dependencies (layer caching)
COPY ["RollUp.csproj", "./"]
RUN dotnet restore "RollUp.csproj"

# Copy all source files and compile release artifact
COPY . .
RUN dotnet publish "RollUp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Configure default runtime environment variables
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Expose internal application container port
EXPOSE 8080

# Copy compiled binaries from build stage
COPY --from=build /app/publish .

# Default container entrypoint
ENTRYPOINT ["dotnet", "RollUp.dll"]
