ARG PARENT_VERSION=dotnet8.0

# Development
FROM defradigital/dotnetcore-development:${PARENT_VERSION} AS development
ARG PARENT_VERSION
LABEL uk.gov.defra.ffc.parent-image=defradigital/dotnetcore-development:${PARENT_VERSION}

COPY --chown=dotnet:dotnet ./src/Directory.Build.props ./Directory.Build.props
RUN mkdir -p /home/dotnet/ncea-enricher/ /home/dotnet/ncea-enricher.tests/
COPY --chown=dotnet:dotnet ./src/ncea-enricher.tests/*.csproj ./ncea-enricher.tests/
RUN dotnet restore ./ncea-enricher.tests/ncea-enricher.tests.csproj
COPY --chown=dotnet:dotnet ./src/ncea-enricher/*.csproj ./ncea-enricher/
RUN dotnet restore ./ncea-enricher/ncea-enricher.csproj
COPY --chown=dotnet:dotnet ./src/ncea-enricher.tests/ ./ncea-enricher.tests/
# some CI builds fail with back to back COPY statements, eg Azure DevOps
RUN true
COPY --chown=dotnet:dotnet ./src/ncea-enricher/ ./ncea-enricher/
RUN dotnet publish ./ncea-enricher/ -c Release -o /home/dotnet/out

ARG PORT=8080
ENV PORT ${PORT}
EXPOSE ${PORT}
# Override entrypoint using shell form so that environment variables are picked up
ENTRYPOINT dotnet watch --project ./ncea-enricher run --urls "http://*:${PORT}"

# Production
FROM defradigital/dotnetcore:${PARENT_VERSION} AS production
ARG PARENT_VERSION
LABEL uk.gov.defra.ffc.parent-image=defradigital/dotnetcore:${PARENT_VERSION}
COPY --from=development /home/dotnet/out/ ./
ARG PORT=8080
ENV ASPNETCORE_URLS http://*:${PORT}
EXPOSE ${PORT}
# Override entrypoint using shell form so that environment variables are picked up
ENTRYPOINT dotnet ncea-enricher.dll
