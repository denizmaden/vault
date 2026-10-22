namespace Vault.Platform.Messaging.Abstractions;

public interface IMessageSubscriber
{
    Task SubscribeAsync(
        string destination,
        string consumerGroup,
        Func<MessageEnvelope, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default);
}
