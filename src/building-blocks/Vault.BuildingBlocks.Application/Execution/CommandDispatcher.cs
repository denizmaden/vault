using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Application.Exceptions;
using Vault.BuildingBlocks.Application.Telemetry;

namespace Vault.BuildingBlocks.Application.Execution;

public sealed class CommandDispatcher(
    IServiceProvider serviceProvider,
    ILogger<CommandDispatcher> logger) : ICommandDispatcher
{
    public async Task DispatchAsync<TCommand>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        ArgumentNullException.ThrowIfNull(command);

        var commandName = typeof(TCommand).Name;
        using var activity = ApplicationTelemetry.StartRequest("command", commandName);
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["request.kind"] = "command",
            ["request.name"] = commandName
        });

        var started = Stopwatch.GetTimestamp();
        logger.LogInformation("Dispatching command {CommandType}.", commandName);

        try
        {
            await ValidateAsync(command, cancellationToken);

            var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
            await handler.Handle(command, cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            ApplicationTelemetry.RecordSuccess("command", commandName, Stopwatch.GetElapsedTime(started));
        }
        catch (RequestValidationException exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "validation_failed");
            activity?.SetTag("vault.validation.error_count", exception.Errors.Count);
            ApplicationTelemetry.RecordValidationFailure("command", commandName, exception.Errors.Count);
            ApplicationTelemetry.RecordFailure("command", commandName, Stopwatch.GetElapsedTime(started), "validation");
            throw;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
            ApplicationTelemetry.RecordFailure("command", commandName, Stopwatch.GetElapsedTime(started), exception.GetType().Name);
            throw;
        }
    }

    public async Task<TResponse> DispatchAsync<TCommand, TResponse>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResponse>
    {
        ArgumentNullException.ThrowIfNull(command);

        var commandName = typeof(TCommand).Name;
        using var activity = ApplicationTelemetry.StartRequest("command", commandName);
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["request.kind"] = "command",
            ["request.name"] = commandName
        });

        var started = Stopwatch.GetTimestamp();
        logger.LogInformation("Dispatching command {CommandType}.", commandName);

        try
        {
            await ValidateAsync(command, cancellationToken);

            var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResponse>>();
            var response = await handler.Handle(command, cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            ApplicationTelemetry.RecordSuccess("command", commandName, Stopwatch.GetElapsedTime(started));

            return response;
        }
        catch (RequestValidationException exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "validation_failed");
            activity?.SetTag("vault.validation.error_count", exception.Errors.Count);
            ApplicationTelemetry.RecordValidationFailure("command", commandName, exception.Errors.Count);
            ApplicationTelemetry.RecordFailure("command", commandName, Stopwatch.GetElapsedTime(started), "validation");
            throw;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
            ApplicationTelemetry.RecordFailure("command", commandName, Stopwatch.GetElapsedTime(started), exception.GetType().Name);
            throw;
        }
    }

    private async Task ValidateAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
    {
        var validators = serviceProvider.GetServices<IRequestValidator<TRequest>>().ToArray();
        if (validators.Length == 0)
        {
            return;
        }

        var errors = new List<Vault.BuildingBlocks.Domain.Primitives.Error>();
        foreach (var validator in validators)
        {
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            errors.AddRange(validationErrors);
        }

        if (errors.Count > 0)
        {
            throw new RequestValidationException(errors);
        }
    }
}
