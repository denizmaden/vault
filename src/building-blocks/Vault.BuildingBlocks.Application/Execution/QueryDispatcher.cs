using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Application.Exceptions;
using Vault.BuildingBlocks.Application.Telemetry;

namespace Vault.BuildingBlocks.Application.Execution;

public sealed class QueryDispatcher(
    IServiceProvider serviceProvider,
    ILogger<QueryDispatcher> logger) : IQueryDispatcher
{
    public async Task<TResponse> DispatchAsync<TQuery, TResponse>(
        TQuery query,
        CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResponse>
    {
        ArgumentNullException.ThrowIfNull(query);

        var queryName = typeof(TQuery).Name;
        using var activity = ApplicationTelemetry.StartRequest("query", queryName);
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["request.kind"] = "query",
            ["request.name"] = queryName
        });

        var started = Stopwatch.GetTimestamp();
        logger.LogInformation("Dispatching query {QueryType}.", queryName);

        try
        {
            await ValidateAsync(query, cancellationToken);

            var handler = serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();
            var response = await handler.Handle(query, cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            ApplicationTelemetry.RecordSuccess("query", queryName, Stopwatch.GetElapsedTime(started));

            return response;
        }
        catch (RequestValidationException exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "validation_failed");
            activity?.SetTag("vault.validation.error_count", exception.Errors.Count);
            ApplicationTelemetry.RecordValidationFailure("query", queryName, exception.Errors.Count);
            ApplicationTelemetry.RecordFailure("query", queryName, Stopwatch.GetElapsedTime(started), "validation");
            throw;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
            ApplicationTelemetry.RecordFailure("query", queryName, Stopwatch.GetElapsedTime(started), exception.GetType().Name);
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
