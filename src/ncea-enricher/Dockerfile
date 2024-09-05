#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

USER root
RUN apt-get -y update && apt-get -y install --no-install-recommends curl \
    mkdir -p /app/Text/Sswe \
	curl -o -L /app/Text/Sswe/sentiment.emd https://aka.ms/mlnet-resources/Text/Sswe/sentiment.emd
	chown -R app:app /app/Text/Sswe \

USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ncea-enricher.csproj", "."]
RUN dotnet restore "./././ncea-enricher.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./ncea-enricher.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ncea-enricher.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

EXPOSE 5000

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ncea-enricher.dll"]