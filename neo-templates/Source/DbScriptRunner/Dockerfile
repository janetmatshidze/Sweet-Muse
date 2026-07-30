# syntax=docker/dockerfile:1
# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
ARG NugetCacheId="nuget"

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:10.0.8-noble-amd64 AS base
WORKDIR /app

RUN apt-get update && apt-get install -y ca-certificates && \
    update-ca-certificates
# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0.301-noble-amd64 AS build
ARG BUILD_CONFIGURATION=Release
ARG NugetCacheId
WORKDIR /src

# Copy in the Directory.Packages.props file to enable Central Package Management
COPY Directory.Packages.props ./

# Build Speed Optimisation: Copy Solution (.sln, .slnx), EditorConfig (.editorconfig) and Project (.csproj) files first
# This allows the package restore to be done on a distinct layer, meaning if your dependencies don't change, a cached copy can be used.
# =================================================================================================
COPY ["NeoTemplate.DbScriptRunner/NeoTemplate.DbScriptRunner.csproj", "NeoTemplate.DbScriptRunner/"]
COPY ["Directory.Packages.props", "."]
RUN --mount=type=cache,id=${NugetCacheId},target=/root/.nuget/packages,sharing=shared \
    --mount=type=cache,id=${NugetCacheId},target=/root/.local/share/NuGet/v3-cache,sharing=shared\
    --mount=type=cache,id=${NugetCacheId},target=/root/.local/share/NuGet/plugins-cache,sharing=shared \
    --mount=type=cache,id=${NugetCacheId},target=/tmp/NuGetScratch,sharing=shared \
    --mount=type=secret,id=NugetConfig \
    dotnet restore "./NeoTemplate.DbScriptRunner/NeoTemplate.DbScriptRunner.csproj" --configfile /run/secrets/NugetConfig


COPY . .
WORKDIR "/src/NeoTemplate.DbScriptRunner"
RUN --mount=type=cache,id=${NugetCacheId},target=/root/.nuget/packages,sharing=shared \
    --mount=type=cache,id=${NugetCacheId},target=/root/.local/share/NuGet/v3-cache,sharing=shared\
    --mount=type=cache,id=${NugetCacheId},target=/root/.local/share/NuGet/plugins-cache,sharing=shared \
    --mount=type=cache,id=${NugetCacheId},target=/tmp/NuGetScratch,sharing=shared \
    --mount=type=secret,id=NugetConfig \
    dotnet build "./NeoTemplate.DbScriptRunner.csproj" -c $BUILD_CONFIGURATION -o /app/build --configfile /run/secrets/NugetConfig

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG NugetCacheId
RUN --mount=type=cache,id=${NugetCacheId},target=/root/.nuget/packages,sharing=shared \
    --mount=type=cache,id=${NugetCacheId},target=/root/.local/share/NuGet/v3-cache,sharing=shared\
    --mount=type=cache,id=${NugetCacheId},target=/root/.local/share/NuGet/plugins-cache,sharing=shared \
    --mount=type=cache,id=${NugetCacheId},target=/tmp/NuGetScratch,sharing=shared \
    --mount=type=secret,id=NugetConfig \
    dotnet publish "./NeoTemplate.DbScriptRunner.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --configfile /run/secrets/NugetConfig

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["sh", "-c", "export CONNECTION_STRING=$(cat /run/secrets/ConnectionString); if [ -f /run/secrets/AzureClientSecret ]; then export AZURE_CLIENT_SECRET=$(cat /run/secrets/AzureClientSecret); fi; exec dotnet NeoTemplate.DbScriptRunner.dll"]
