using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Web;

namespace Vault.Platform.Observability.Logging.NLog;

public static class DependencyInjection
{
    public static IHostBuilder AddVaultNLogLogging(
        this IHostBuilder hostBuilder,
        string serviceName)
    {
        var jsonLayout = new JsonLayout
        {
            IncludeEventProperties = true,
            Attributes =
            {
                new JsonAttribute("timestamp", "${date:universalTime=true:format=o}"),
                new JsonAttribute("level", "${level:uppercase=true}"),
                new JsonAttribute("message", "${message:raw=true}"),
                new JsonAttribute("logger", "${logger}"),
                new JsonAttribute("service", serviceName),
                new JsonAttribute("environment", "${host-environment}"),
                new JsonAttribute("exception", "${exception:format=tostring}"),
                new JsonAttribute("trace_id", "${activity:property=TraceId}"),
                new JsonAttribute("span_id", "${activity:property=SpanId}")
            }
        };

        var consoleTarget = new ConsoleTarget("console")
        {
            Layout = jsonLayout
        };

        var configuration = new LoggingConfiguration();
        configuration.AddRule(global::NLog.LogLevel.Info, global::NLog.LogLevel.Fatal, consoleTarget);
        LogManager.Configuration = configuration;

        hostBuilder.UseNLog();

        return hostBuilder;
    }
}
