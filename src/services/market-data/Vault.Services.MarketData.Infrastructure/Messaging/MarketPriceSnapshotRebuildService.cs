using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vault.Services.MarketData.Application.Abstractions;
using Vault.Services.MarketData.Infrastructure.Configuration;

namespace Vault.Services.MarketData.Infrastructure.Messaging;

public sealed class MarketPriceSnapshotRebuildService(
    IMarketPriceSnapshotStore snapshotStore,
    IMarketInstrumentCatalog instrumentCatalog,
    MarketDataMessagingSettings messagingSettings,
    MarketDataKafkaSettings kafkaSettings,
    ILogger<MarketPriceSnapshotRebuildService> logger) : IHostedService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!messagingSettings.RebuildSnapshotWhenEmpty)
        {
            return;
        }

        var existingSnapshot = await snapshotStore.GetSnapshotAsync(null, cancellationToken);
        if (existingSnapshot.Count > 0)
        {
            logger.LogInformation(
                "MarketData Redis snapshot contains {PriceCount} prices; Kafka rebuild is not required.",
                existingSnapshot.Count);
            return;
        }

        logger.LogWarning("MarketData Redis snapshot is empty. Rebuilding it from Kafka.");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(messagingSettings.RebuildTimeoutSeconds));

        try
        {
            var replayedEventCount = await RebuildAsync(timeout.Token);
            var rebuiltSnapshot = await snapshotStore.GetSnapshotAsync(null, timeout.Token);
            logger.LogInformation(
                "MarketData Redis snapshot rebuild completed after accepting {EventCount} events; {PriceCount} current prices are available.",
                replayedEventCount,
                rebuiltSnapshot.Count);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"MarketData Redis snapshot rebuild exceeded {messagingSettings.RebuildTimeoutSeconds} seconds.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task<int> RebuildAsync(CancellationToken cancellationToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = kafkaSettings.BootstrapServers,
            GroupId = $"vault-market-data-rebuild-{Guid.NewGuid():N}",
            EnableAutoCommit = false,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            AllowAutoCreateTopics = false
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        using var adminClient = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = kafkaSettings.BootstrapServers
        }).Build();
        var metadata = adminClient.GetMetadata(
            messagingSettings.PricesDestination,
            TimeSpan.FromSeconds(10));
        var topic = metadata.Topics.SingleOrDefault(item =>
            string.Equals(item.Topic, messagingSettings.PricesDestination, StringComparison.Ordinal));

        if (topic is null || topic.Error.IsError)
        {
            throw new InvalidOperationException(
                $"Unable to read Kafka metadata for '{messagingSettings.PricesDestination}'.");
        }

        var partitions = topic.Partitions
            .Select(partition => new TopicPartition(
                messagingSettings.PricesDestination,
                new Partition(partition.PartitionId)))
            .ToArray();
        var targetOffsets = partitions.ToDictionary(
            partition => partition,
            partition => consumer.QueryWatermarkOffsets(partition, TimeSpan.FromSeconds(10)).High.Value);
        var currentOffsets = partitions.ToDictionary(
            partition => partition,
            partition => consumer.QueryWatermarkOffsets(partition, TimeSpan.FromSeconds(10)).Low.Value);

        consumer.Assign(partitions.Select(partition =>
            new TopicPartitionOffset(partition, Offset.Beginning)));

        var rebuiltCount = 0;
        while (targetOffsets.Any(target => currentOffsets[target.Key] < target.Value))
        {
            var result = consumer.Consume(cancellationToken);
            if (result?.Message is null)
            {
                continue;
            }

            currentOffsets[result.TopicPartition] = result.Offset.Value + 1;

            MarketPriceUpdatedIntegrationEvent? @event;
            try
            {
                @event = JsonSerializer.Deserialize<MarketPriceUpdatedIntegrationEvent>(
                    result.Message.Value,
                    JsonSerializerOptions);
            }
            catch (JsonException exception)
            {
                logger.LogWarning(
                    exception,
                    "MarketData Redis rebuild skipped invalid Kafka record at {TopicPartitionOffset}.",
                    result.TopicPartitionOffset);
                continue;
            }

            if (@event is null || !instrumentCatalog.Contains(@event.Symbol))
            {
                continue;
            }

            if (await snapshotStore.UpsertAsync(@event.ToDomain(), cancellationToken))
            {
                rebuiltCount++;
            }
        }

        consumer.Close();
        return rebuiltCount;
    }
}
