using Confluent.Kafka;
using Vault.Platform.Messaging.Kafka.Abstractions;
using Vault.Platform.Messaging.Kafka.Configuration;

namespace Vault.Platform.Messaging.Kafka;

internal sealed class KafkaConsumerFactory(KafkaProducerSettings settings) : IKafkaConsumerFactory
{
    public IConsumer<string, string> Create(string groupId)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = settings.BootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            AllowAutoCreateTopics = true
        };

        return new ConsumerBuilder<string, string>(config).Build();
    }
}

