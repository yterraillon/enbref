# Base non-alpine et sans `USER $APP_UID` : c'est ce qui fonctionne avec le volume du NAS
# et son couple PUID/PGID.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
# wget sert au healthcheck de infra/compose.local.yml ; l'image aspnet n'en fournit pas.
RUN apt-get update && apt-get install -y --no-install-recommends wget && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
# Pas « VERSION » : MSBuild lirait la variable d'environnement comme $(Version).
ARG APP_VERSION=dev
# Contexte de build : la racine du dépôt, pour atteindre design-system/ (ADR-009).
WORKDIR /src
COPY server/Directory.Packages.props server/Directory.Build.props server/global.json ./server/
COPY server/src/Api/Api.csproj ./server/src/Api/
COPY server/src/Infrastructure/Infrastructure.csproj ./server/src/Infrastructure/
RUN dotnet restore ./server/src/Api/Api.csproj
COPY server/ ./server/
COPY design-system/ ./design-system/
RUN dotnet publish ./server/src/Api/Api.csproj -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false /p:InformationalVersion=$APP_VERSION

FROM base AS final
WORKDIR /app
ARG APP_VERSION=dev
LABEL version=$APP_VERSION
LABEL org.opencontainers.image.version=$APP_VERSION
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
