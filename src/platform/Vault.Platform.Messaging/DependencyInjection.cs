using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vault.Platform.Messaging.Configuration;
using Vault.Platform.Messaging.Kafka;

namespace Vault.Platform.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddVaultMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        string clientId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);

        var options = configuration
            .GetSection(VaultMessagingOptions.SectionName)
            .Get<VaultMessagingOptions>()
            ?? new VaultMessagingOptions();

        if (string.IsNullOrWhiteSpace(options.Provider))
        {
            throw new InvalidOperationException(
                "Messaging provider is missing. Supported providers: Kafka.");
        }

        switch (options.Provider.Trim().ToUpperInvariant())
        {
            case "KAFKA":
                if (string.IsNullOrWhiteSpace(options.Kafka.BootstrapServers))
                {
                    throw new InvalidOperationException(
                        "Messaging:Kafka:BootstrapServers must be configured for the Kafka provider.");
                }

                services.AddVaultKafkaMessaging(options.Kafka.BootstrapServers, clientId);
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported messaging provider '{options.Provider}'. Supported providers: Kafka.");
        }

        return services;
    }
}
