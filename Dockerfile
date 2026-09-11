# ============================================================
# Build stage
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution + project files for restore (layer caching)
COPY DigitalFormsSystem.sln ./
COPY DigitalFormsSystem.csproj ./
COPY DigitalFormsSystem.Core/DigitalFormsSystem.Core.csproj ./DigitalFormsSystem.Core/

# Restore
RUN dotnet restore ./DigitalFormsSystem.csproj

# Copy full source
COPY . .

# Publish release build
RUN dotnet publish ./DigitalFormsSystem.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ============================================================
# Runtime stage
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Environment defaults
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Render provides $PORT at runtime — use it, fallback to 8080 for local docker
ENTRYPOINT ["sh", "-c", "dotnet DigitalFormsSystem.dll --urls http://0.0.0.0:${PORT:-8080}"]