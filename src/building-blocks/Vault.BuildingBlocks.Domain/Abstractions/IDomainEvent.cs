namespace Vault.BuildingBlocks.Domain.Abstractions;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
