# ── Stage 1: Build ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore dependencies (cached layer when only csproj changes)
COPY SecureTaskApi.csproj ./
RUN dotnet restore SecureTaskApi.csproj

# Copy remaining source and publish
COPY . ./
RUN dotnet publish SecureTaskApi.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# ── Stage 2: Runtime ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install EF Core CLI tool for applying migrations at startup
COPY --from=build /app/publish ./

# Render injects $PORT; ASP.NET Core reads ASPNETCORE_URLS at startup
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production

# The following secrets must be set as environment variables in the Render dashboard:
#   DATABASE_URL   – PostgreSQL connection string (e.g. Host=...;Database=...;Username=...;Password=...)
#   JWT_KEY        – Secret key used to sign JWT tokens
#   JWT_ISSUER     – (optional) defaults to "SecureTaskApi"
#   JWT_AUDIENCE   – (optional) defaults to "SecureTaskApiUser"

EXPOSE 8080

ENTRYPOINT ["dotnet", "SecureTaskApi.dll"]
