using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Vault.Platform.Observability;

internal sealed class VaultRequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<VaultRequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var started = Stopwatch.GetTimestamp();
        Exception? exception = null;

        try
        {
            await next(context);
        }
        catch (Exception caughtException)
        {
            exception = caughtException;
            throw;
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(started);
            var statusCode = context.Response.StatusCode;
            var logLevel = exception is not null || statusCode >= StatusCodes.Status500InternalServerError
                ? LogLevel.Error
                : statusCode >= StatusCodes.Status400BadRequest
                    ? LogLevel.Warning
                    : LogLevel.Information;

            var activity = Activity.Current;

            logger.Log(
                logLevel,
                exception,
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {ElapsedMilliseconds:0.0000} ms. " +
                "CorrelationId: {CorrelationId}, Route: {Route}, UserAuthenticated: {UserAuthenticated}, " +
                "TraceId: {TraceId}, SpanId: {SpanId}",
                context.Request.Method,
                context.Request.Path.Value,
                statusCode,
                elapsed.TotalMilliseconds,
                context.TraceIdentifier,
                context.GetEndpoint()?.DisplayName ?? "unknown",
                context.User.Identity?.IsAuthenticated == true,
                activity?.TraceId.ToString() ?? string.Empty,
                activity?.SpanId.ToString() ?? string.Empty);
        }
    }
}
