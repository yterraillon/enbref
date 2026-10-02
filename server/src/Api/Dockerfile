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
WORKDIR /src
COPY ./Directory.Packages.props ./Directory.Build.props ./global.json ./
COPY ./src/Api/Api.csproj ./src/Api/
COPY ./src/Infrastructure/Infrastructure.csproj ./src/Infrastructure/
RUN dotnet restore ./src/Api/Api.csproj
COPY . .
RUN dotnet publish ./src/Api/Api.csproj -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false /p:InformationalVersion=$APP_VERSION

FROM base AS final
WORKDIR /app
ARG APP_VERSION=dev
LABEL version=$APP_VERSION
LABEL org.opencontainers.image.version=$APP_VERSION
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
