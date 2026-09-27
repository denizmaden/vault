using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Vault.BuildingBlocks.Api.Correlation;

namespace Vault.BuildingBlocks.Api.Middlewares
{
    public sealed class CorrelationIdMiddleware(
    RequestDelegate next,
    ILogger<CorrelationIdMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context, CorrelationContextAccessor accessor)
        {
            var correlationId = context.Request.Headers[CorrelationConstants.CorrelationIdHeaderName].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString("N");
            }

            accessor.CorrelationId = correlationId;
            context.TraceIdentifier = correlationId;
            context.Response.Headers[CorrelationConstants.CorrelationIdHeaderName] = correlationId;
            Activity.Current?.SetTag("correlation.id", correlationId);

            using var scope = logger.BeginScope(new Dictionary<string, object>
            {
                ["correlation_id"] = correlationId
            });

            await next(context);
        }
    }
}