using Confluent.Kafka;

namespace Vault.Platform.Messaging.Kafka.Abstractions;

internal interface IKafkaConsumerFactory
{
    IConsumer<string, string> Create(string groupId);
}

