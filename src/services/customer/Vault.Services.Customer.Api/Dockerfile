FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["Vault.sln", "./"]
COPY ["src/building-blocks/Vault.BuildingBlocks.Domain/Vault.BuildingBlocks.Domain.csproj", "src/building-blocks/Vault.BuildingBlocks.Domain/"]
COPY ["src/building-blocks/Vault.BuildingBlocks.Application/Vault.BuildingBlocks.Application.csproj", "src/building-blocks/Vault.BuildingBlocks.Application/"]
COPY ["src/building-blocks/Vault.BuildingBlocks.Api/Vault.BuildingBlocks.Api.csproj", "src/building-blocks/Vault.BuildingBlocks.Api/"]
COPY ["src/platform/Vault.Platform.Persistence.Abstractions/Vault.Platform.Persistence.Abstractions.csproj", "src/platform/Vault.Platform.Persistence.Abstractions/"]
COPY ["src/platform/Vault.Platform.Persistence.EntityFramework/Vault.Platform.Persistence.EntityFramework.csproj", "src/platform/Vault.Platform.Persistence.EntityFramework/"]
COPY ["src/services/Customer/Vault.Services.Customer.Domain/Vault.Services.Customer.Domain.csproj", "src/services/Customer/Vault.Services.Customer.Domain/"]
COPY ["src/services/Customer/Vault.Services.Customer.Application/Vault.Services.Customer.Application.csproj", "src/services/Customer/Vault.Services.Customer.Application/"]
COPY ["src/services/Customer/Vault.Services.Customer.Infrastructure/Vault.Services.Customer.Infrastructure.csproj", "src/services/Customer/Vault.Services.Customer.Infrastructure/"]
COPY ["src/services/Customer/Vault.Services.Customer.Api/Vault.Services.Customer.Api.csproj", "src/services/Customer/Vault.Services.Customer.Api/"]

RUN dotnet restore "src/services/Customer/Vault.Services.Customer.Api/Vault.Services.Customer.Api.csproj"

COPY . .
RUN dotnet publish "src/services/Customer/Vault.Services.Customer.Api/Vault.Services.Customer.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Vault.Services.Customer.Api.dll"]
