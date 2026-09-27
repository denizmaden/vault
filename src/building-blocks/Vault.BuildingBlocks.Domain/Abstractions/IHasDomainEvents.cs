namespace Vault.BuildingBlocks.Domain.Abstractions;

public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> GetDomainEvents();
    void ClearDomainEvents();
}
