namespace Vault.BuildingBlocks.Application.Abstractions;

public interface IQueryDispatcher
{
    Task<TResponse> DispatchAsync<TQuery, TResponse>(
        TQuery query,
        CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResponse>;
}
