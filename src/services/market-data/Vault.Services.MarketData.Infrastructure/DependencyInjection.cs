using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Vault.Services.MarketData.Application.Abstractions;
using Vault.Services.MarketData.Infrastructure.Configuration;
using Vault.Services.MarketData.Infrastructure.HealthChecks;
using Vault.Services.MarketData.Infrastructure.Messaging;
using Vault.Services.MarketData.Infrastructure.Persistence;
using Vault.Services.MarketData.Infrastructure.Realtime;
using Vault.Services.MarketData.Infrastructure.Streaming;

namespace Vault.Services.MarketData.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMarketDataInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var finnhubSettings = configuration
            .GetSection(FinnhubMarketDataSettings.SectionName)
            .Get<FinnhubMarketDataSettings>()
            ?? new FinnhubMarketDataSettings();

        var binanceSettings = configuration
            .GetSection(BinanceMarketDataSettings.SectionName)
            .Get<BinanceMarketDataSettings>()
            ?? new BinanceMarketDataSettings();

        var messagingSettings = configuration
            .GetSection(MarketDataMessagingSettings.SectionName)
            .Get<MarketDataMessagingSettings>()
            ?? new MarketDataMessagingSettings();

        var redisSettings = configuration
            .GetSection(MarketDataRedisSettings.SectionName)
            .Get<MarketDataRedisSettings>()
            ?? new MarketDataRedisSettings();

        var ingestionSettings = configuration
            .GetSection(MarketDataIngestionSettings.SectionName)
            .Get<MarketDataIngestionSettings>()
            ?? new MarketDataIngestionSettings();

        var messagingProvider = configuration["Messaging:Provider"];
        var kafkaSettings = new MarketDataKafkaSettings
        {
            BootstrapServers = configuration["Messaging:Kafka:BootstrapServers"] ?? string.Empty
        };

        if (string.IsNullOrWhiteSpace(redisSettings.ConnectionString))
        {
            throw new InvalidOperationException("MarketData Redis connection string is missing.");
        }

        if (!string.Equals(messagingProvider, "Kafka", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(kafkaSettings.BootstrapServers))
        {
            throw new InvalidOperationException("MarketData requires the Kafka messaging provider.");
        }

        if (string.IsNullOrWhiteSpace(messagingSettings.PricesDestination)
            || string.IsNullOrWhiteSpace(messagingSettings.DeadLetterDestination)
            || string.IsNullOrWhiteSpace(messagingSettings.ProjectionConsumerGroupId)
            || messagingSettings.MaxProcessingAttempts <= 0
            || messagingSettings.RetryInitialDelayMilliseconds <= 0
            || messagingSettings.DeadLetterPublishRetrySeconds <= 0
            || messagingSettings.RebuildTimeoutSeconds <= 0)
        {
            throw new InvalidOperationException("MarketData messaging settings are incomplete.");
        }

        services.AddSingleton(finnhubSettings);
        services.AddSingleton(binanceSettings);
        services.AddSingleton(messagingSettings);
        services.AddSingleton(redisSettings);
        services.AddSingleton(ingestionSettings);
        services.AddSingleton(kafkaSettings);
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisSettings.ConnectionString));
        services.AddSingleton<IMarketInstrumentCatalog, MarketInstrumentCatalog>();
        services.AddSingleton<IMarketPriceSnapshotStore, RedisMarketPriceSnapshotStore>();
        services.AddSingleton<IMarketPriceRealtimeBackplane, RedisMarketPriceRealtimeBackplane>();
        services.AddSingleton<IMarketPricePublisher, KafkaMarketPricePublisher>();
        services.AddHostedService<MarketPriceSnapshotRebuildService>();
        services.AddHostedService<MarketPriceProjectionConsumer>();
        services.AddHostedService<MarketPriceBroadcastService>();

        if (ingestionSettings.Enabled)
        {
            services.AddHostedService<FinnhubForexWebSocketClient>();
            services.AddHostedService<BinanceCryptoWebSocketClient>();
        }

        services.AddHealthChecks()
            .AddCheck<RedisMarketDataHealthCheck>("market-data-redis", tags: ["ready"]);

        return services;
    }
}
