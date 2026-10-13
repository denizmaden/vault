using Microsoft.AspNetCore.Builder;

namespace Vault.Platform.Observability;

public static class VaultObservabilityApplicationBuilderExtensions
{
    public static IApplicationBuilder UseVaultObservability(this IApplicationBuilder app)
    {
        return app.UseMiddleware<VaultRequestLoggingMiddleware>();
    }
}
