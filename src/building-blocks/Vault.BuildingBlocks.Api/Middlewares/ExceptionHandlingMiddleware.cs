using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Vault.BuildingBlocks.Application.Exceptions;
using Vault.BuildingBlocks.Api.Correlation;
using Microsoft.AspNetCore.Mvc;

namespace Vault.BuildingBlocks.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, CorrelationContextAccessor correlationContextAccessor)
    {
        try
        {
            await next(context);
        }
        catch (RequestValidationException exception)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Title = "Request validation failed.",
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more validation errors occurred.",
                Extensions =
                {
                    ["correlationId"] = correlationContextAccessor.CorrelationId,
                    ["errors"] = exception.Errors.Select(error => new
                    {
                        error.Code,
                        error.Description
                    }).ToArray()
                }
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception while processing request.");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Title = "An unexpected error occurred.",
                Status = StatusCodes.Status500InternalServerError,
                Extensions =
                {
                    ["correlationId"] = correlationContextAccessor.CorrelationId
                }
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
    }
}
