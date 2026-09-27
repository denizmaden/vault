namespace Vault.BuildingBlocks.Application.Abstractions;

public interface ICommandDispatcher
{
    Task DispatchAsync<TCommand>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : ICommand;

    Task<TResponse> DispatchAsync<TCommand, TResponse>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResponse>;
}
