namespace Vault.Platform.Messaging.Abstractions;

public interface IMessagePublisher
{
    Task PublishAsync(MessageEnvelope envelope, CancellationToken cancellationToken = default);
}

