namespace Vault.BuildingBlocks.Domain.Primitives;

public abstract class Entity<TId>(TId id)
    where TId : notnull
{
    public TId Id { get; } = id;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other || GetType() != other.GetType())
        {
            return false;
        }

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
