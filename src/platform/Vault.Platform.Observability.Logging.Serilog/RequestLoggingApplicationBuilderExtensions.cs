using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Events;

namespace Vault.Platform.Observability.Logging.Serilog;

public static class RequestLoggingApplicationBuilderExtensions
{
    public static IApplicationBuilder UseVaultSerilogRequestLogging(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.GetLevel = (httpContext, _, exception) =>
            {
                if (exception is not null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError)
                {
                    return LogEventLevel.Error;
                }

                return httpContext.Response.StatusCode >= StatusCodes.Status400BadRequest
                    ? LogEventLevel.Warning
                    : LogEventLevel.Information;
            };

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("correlation_id", httpContext.TraceIdentifier);
                diagnosticContext.Set("http.route", httpContext.GetEndpoint()?.DisplayName ?? "unknown");
                diagnosticContext.Set("user.authenticated", httpContext.User.Identity?.IsAuthenticated == true);

                var activity = Activity.Current;
                if (activity is not null)
                {
                    diagnosticContext.Set("trace_id", activity.TraceId.ToString());
                    diagnosticContext.Set("span_id", activity.SpanId.ToString());
                }
            };
        });

        return app;
    }
}
