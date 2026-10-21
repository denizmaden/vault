FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/building-blocks/Vault.BuildingBlocks.Domain/Vault.BuildingBlocks.Domain.csproj", "src/building-blocks/Vault.BuildingBlocks.Domain/"]
COPY ["src/building-blocks/Vault.BuildingBlocks.Application/Vault.BuildingBlocks.Application.csproj", "src/building-blocks/Vault.BuildingBlocks.Application/"]
COPY ["src/building-blocks/Vault.BuildingBlocks.Api/Vault.BuildingBlocks.Api.csproj", "src/building-blocks/Vault.BuildingBlocks.Api/"]
COPY ["src/platform/Vault.Platform.Persistence.Abstractions/Vault.Platform.Persistence.Abstractions.csproj", "src/platform/Vault.Platform.Persistence.Abstractions/"]
COPY ["src/platform/Vault.Platform.Persistence.EntityFramework/Vault.Platform.Persistence.EntityFramework.csproj", "src/platform/Vault.Platform.Persistence.EntityFramework/"]
COPY ["src/platform/Vault.Platform.Observability.Logging.NLog/Vault.Platform.Observability.Logging.NLog.csproj", "src/platform/Vault.Platform.Observability.Logging.NLog/"]
COPY ["src/platform/Vault.Platform.Observability.Logging.Serilog/Vault.Platform.Observability.Logging.Serilog.csproj", "src/platform/Vault.Platform.Observability.Logging.Serilog/"]
COPY ["src/platform/Vault.Platform.Observability.Metrics.OpenTelemetry/Vault.Platform.Observability.Metrics.OpenTelemetry.csproj", "src/platform/Vault.Platform.Observability.Metrics.OpenTelemetry/"]
COPY ["src/platform/Vault.Platform.Observability.Tracing.OpenTelemetry/Vault.Platform.Observability.Tracing.OpenTelemetry.csproj", "src/platform/Vault.Platform.Observability.Tracing.OpenTelemetry/"]
COPY ["src/platform/Vault.Platform.Observability/Vault.Platform.Observability.csproj", "src/platform/Vault.Platform.Observability/"]
COPY ["src/services/customer/Vault.Services.Customer.Domain/Vault.Services.Customer.Domain.csproj", "src/services/customer/Vault.Services.Customer.Domain/"]
COPY ["src/services/customer/Vault.Services.Customer.Application/Vault.Services.Customer.Application.csproj", "src/services/customer/Vault.Services.Customer.Application/"]
COPY ["src/services/customer/Vault.Services.Customer.Infrastructure/Vault.Services.Customer.Infrastructure.csproj", "src/services/customer/Vault.Services.Customer.Infrastructure/"]
COPY ["src/services/customer/Vault.Services.Customer.Api/Vault.Services.Customer.Api.csproj", "src/services/customer/Vault.Services.Customer.Api/"]

RUN dotnet restore "src/services/customer/Vault.Services.Customer.Api/Vault.Services.Customer.Api.csproj"

COPY . .
RUN dotnet publish "src/services/customer/Vault.Services.Customer.Api/Vault.Services.Customer.Api.csproj" \
    --configuration "$BUILD_CONFIGURATION" \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

USER $APP_UID
ENTRYPOINT ["dotnet", "Vault.Services.Customer.Api.dll"]
