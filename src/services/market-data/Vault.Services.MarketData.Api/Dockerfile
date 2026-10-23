FROM node:24-alpine AS client-build
WORKDIR /app

COPY ["src/services/market-data/Vault.Services.MarketData.Api/ClientApp/package.json", "./"]
COPY ["src/services/market-data/Vault.Services.MarketData.Api/ClientApp/package-lock.json", "./"]
RUN npm ci

COPY ["src/services/market-data/Vault.Services.MarketData.Api/ClientApp/", "./"]
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/building-blocks/Vault.BuildingBlocks.Domain/Vault.BuildingBlocks.Domain.csproj", "src/building-blocks/Vault.BuildingBlocks.Domain/"]
COPY ["src/building-blocks/Vault.BuildingBlocks.Application/Vault.BuildingBlocks.Application.csproj", "src/building-blocks/Vault.BuildingBlocks.Application/"]
COPY ["src/building-blocks/Vault.BuildingBlocks.Api/Vault.BuildingBlocks.Api.csproj", "src/building-blocks/Vault.BuildingBlocks.Api/"]
COPY ["src/platform/Vault.Platform.Messaging.Abstractions/Vault.Platform.Messaging.Abstractions.csproj", "src/platform/Vault.Platform.Messaging.Abstractions/"]
COPY ["src/platform/Vault.Platform.Messaging.Kafka/Vault.Platform.Messaging.Kafka.csproj", "src/platform/Vault.Platform.Messaging.Kafka/"]
COPY ["src/platform/Vault.Platform.Messaging.RabbitMQ/Vault.Platform.Messaging.RabbitMQ.csproj", "src/platform/Vault.Platform.Messaging.RabbitMQ/"]
COPY ["src/platform/Vault.Platform.Messaging/Vault.Platform.Messaging.csproj", "src/platform/Vault.Platform.Messaging/"]
COPY ["src/platform/Vault.Platform.Observability.Logging.NLog/Vault.Platform.Observability.Logging.NLog.csproj", "src/platform/Vault.Platform.Observability.Logging.NLog/"]
COPY ["src/platform/Vault.Platform.Observability.Logging.Serilog/Vault.Platform.Observability.Logging.Serilog.csproj", "src/platform/Vault.Platform.Observability.Logging.Serilog/"]
COPY ["src/platform/Vault.Platform.Observability.Metrics.OpenTelemetry/Vault.Platform.Observability.Metrics.OpenTelemetry.csproj", "src/platform/Vault.Platform.Observability.Metrics.OpenTelemetry/"]
COPY ["src/platform/Vault.Platform.Observability.Tracing.OpenTelemetry/Vault.Platform.Observability.Tracing.OpenTelemetry.csproj", "src/platform/Vault.Platform.Observability.Tracing.OpenTelemetry/"]
COPY ["src/platform/Vault.Platform.Observability/Vault.Platform.Observability.csproj", "src/platform/Vault.Platform.Observability/"]
COPY ["src/services/market-data/Vault.Services.MarketData.Domain/Vault.Services.MarketData.Domain.csproj", "src/services/market-data/Vault.Services.MarketData.Domain/"]
COPY ["src/services/market-data/Vault.Services.MarketData.Application/Vault.Services.MarketData.Application.csproj", "src/services/market-data/Vault.Services.MarketData.Application/"]
COPY ["src/services/market-data/Vault.Services.MarketData.Infrastructure/Vault.Services.MarketData.Infrastructure.csproj", "src/services/market-data/Vault.Services.MarketData.Infrastructure/"]
COPY ["src/services/market-data/Vault.Services.MarketData.Api/Vault.Services.MarketData.Api.csproj", "src/services/market-data/Vault.Services.MarketData.Api/"]

RUN dotnet restore "src/services/market-data/Vault.Services.MarketData.Api/Vault.Services.MarketData.Api.csproj"

COPY . .
COPY --from=client-build /wwwroot ./src/services/market-data/Vault.Services.MarketData.Api/wwwroot
RUN dotnet publish "src/services/market-data/Vault.Services.MarketData.Api/Vault.Services.MarketData.Api.csproj" \
    --configuration "$BUILD_CONFIGURATION" \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

USER $APP_UID
ENTRYPOINT ["dotnet", "Vault.Services.MarketData.Api.dll"]
