# Base non-alpine et sans `USER $APP_UID`, comme l'image myfanwy dont EnBref est issu :
# c'est ce qui fonctionne avec le volume du NAS et son couple PUID/PGID.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ./Directory.Packages.props ./Directory.Build.props ./global.json ./
COPY ./src/Api/Api.csproj ./src/Api/
COPY ./src/BuildingBlocks/Application/Application.csproj ./src/BuildingBlocks/Application/
COPY ./src/BuildingBlocks/Infrastructure/Infrastructure.csproj ./src/BuildingBlocks/Infrastructure/
COPY ./src/Modules/EnBref/EnBref.Application/EnBref.Application.csproj ./src/Modules/EnBref/EnBref.Application/
COPY ./src/Modules/EnBref/EnBref.Infrastructure/EnBref.Infrastructure.csproj ./src/Modules/EnBref/EnBref.Infrastructure/
RUN dotnet restore "./src/Api/Api.csproj"
COPY . .
WORKDIR "/src/src/Api"
RUN dotnet build "Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/src/Api
RUN dotnet publish "Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ARG VERSION=unknown
LABEL version=$VERSION
LABEL org.opencontainers.image.version=$VERSION
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
