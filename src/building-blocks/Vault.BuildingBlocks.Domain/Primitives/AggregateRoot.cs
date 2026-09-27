using Vault.BuildingBlocks.Domain.Abstractions;

namespace Vault.BuildingBlocks.Domain.Primitives;

public abstract class AggregateRoot<TId>(TId id) : Entity<TId>(id), IHasDomainEvents
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();
}
