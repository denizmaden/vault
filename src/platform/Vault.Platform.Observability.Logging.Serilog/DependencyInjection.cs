using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Vault.Platform.Observability.Logging.Serilog;

public static class DependencyInjection
{
    public static IHostBuilder AddVaultSerilogLogging(
        this IHostBuilder hostBuilder,
        string serviceName)
    {
        hostBuilder.UseSerilog((context, _, configuration) =>
        {
            configuration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.With(new ActivityLogEventEnricher())
                .Enrich.WithProperty("service", serviceName)
                .Enrich.WithProperty("service_name", serviceName)
                .Enrich.WithProperty("environment", context.HostingEnvironment.EnvironmentName)
                .Enrich.WithProperty("deployment_environment", context.HostingEnvironment.EnvironmentName)
                .WriteTo.Console(new JsonFormatter(renderMessage: true));
        });

        return hostBuilder;
    }
}
