FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/building-blocks/Vault.BuildingBlocks.Domain/Vault.BuildingBlocks.Domain.csproj", "src/building-blocks/Vault.BuildingBlocks.Domain/"]
COPY ["src/building-blocks/Vault.BuildingBlocks.Application/Vault.BuildingBlocks.Application.csproj", "src/building-blocks/Vault.BuildingBlocks.Application/"]
COPY ["src/building-blocks/Vault.BuildingBlocks.Api/Vault.BuildingBlocks.Api.csproj", "src/building-blocks/Vault.BuildingBlocks.Api/"]
COPY ["src/platform/Vault.Platform.Persistence.Abstractions/Vault.Platform.Persistence.Abstractions.csproj", "src/platform/Vault.Platform.Persistence.Abstractions/"]
COPY ["src/platform/Vault.Platform.Persistence.EntityFramework/Vault.Platform.Persistence.EntityFramework.csproj", "src/platform/Vault.Platform.Persistence.EntityFramework/"]
COPY ["src/services/identity/Vault.Services.Identity.Domain/Vault.Services.Identity.Domain.csproj", "src/services/identity/Vault.Services.Identity.Domain/"]
COPY ["src/services/identity/Vault.Services.Identity.Application/Vault.Services.Identity.Application.csproj", "src/services/identity/Vault.Services.Identity.Application/"]
COPY ["src/services/identity/Vault.Services.Identity.Infrastructure/Vault.Services.Identity.Infrastructure.csproj", "src/services/identity/Vault.Services.Identity.Infrastructure/"]
COPY ["src/services/identity/Vault.Services.Identity.Api/Vault.Services.Identity.Api.csproj", "src/services/identity/Vault.Services.Identity.Api/"]

RUN dotnet restore "src/services/identity/Vault.Services.Identity.Api/Vault.Services.Identity.Api.csproj"

COPY . .
RUN dotnet publish "src/services/identity/Vault.Services.Identity.Api/Vault.Services.Identity.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Vault.Services.Identity.Api.dll"]
