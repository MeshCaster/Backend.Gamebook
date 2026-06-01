# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY nuget.config .
COPY GameBook.sln .
COPY src/GameBook.Api/GameBook.Api.csproj src/GameBook.Api/
COPY src/GameBook.Application/GameBook.Application.csproj src/GameBook.Application/
COPY src/GameBook.Domain/GameBook.Domain.csproj src/GameBook.Domain/
COPY src/GameBook.Infrastructure/GameBook.Infrastructure.csproj src/GameBook.Infrastructure/
COPY src/GameBook.Contracts/GameBook.Contracts.csproj src/GameBook.Contracts/
COPY tests/GameBook.Application.UnitTests/GameBook.Application.UnitTests.csproj tests/GameBook.Application.UnitTests/
COPY tests/GameBook.Api.IntegrationTests/GameBook.Api.IntegrationTests.csproj tests/GameBook.Api.IntegrationTests/
COPY tests/GameBook.Architecture.Tests/GameBook.Architecture.Tests.csproj tests/GameBook.Architecture.Tests/
RUN --mount=type=secret,id=nuget_token \
    dotnet nuget update source MeshCaster \
      --username "meshcaster" \
      --password "$(cat /run/secrets/nuget_token)" \
      --store-password-in-clear-text \
      --configfile nuget.config \
    && dotnet restore

COPY . .
RUN dotnet publish src/GameBook.Api/GameBook.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 5080
ENTRYPOINT ["dotnet", "GameBook.Api.dll"]
