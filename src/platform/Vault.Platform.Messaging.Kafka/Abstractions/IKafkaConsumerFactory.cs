using Confluent.Kafka;

namespace Vault.Platform.Messaging.Kafka.Abstractions;

public interface IKafkaConsumerFactory
{
    IConsumer<string, string> Create(string groupId);
}

