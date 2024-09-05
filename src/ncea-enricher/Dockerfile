FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

USER root
RUN apt-get -y update && apt-get -y install --no-install-recommends curl 
RUN rm -rf /var/lib/apt/lists/*

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

RUN mkdir -p /app/Text/Sswe
RUN curl -kLo /app/Text/Sswe/sentiment.emd https://aka.ms/mlnet-resources/Text/Sswe/sentiment.emd 

EXPOSE 5000

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ncea-enricher.dll"]