using Microsoft.Extensions.Options;

namespace Vault.Services.Identity.Api.Configuration;

public sealed class IdentityServerSettingsValidator : IValidateOptions<IdentityServerSettings>
{
    public ValidateOptionsResult Validate(string? name, IdentityServerSettings options)
    {
        if (string.IsNullOrWhiteSpace(options.SwaggerClientId))
        {
            return ValidateOptionsResult.Fail("IdentityServer:SwaggerClientId must be configured.");
        }

        if (string.IsNullOrWhiteSpace(options.SwaggerClientSecret))
        {
            return ValidateOptionsResult.Fail("IdentityServer:SwaggerClientSecret must be configured.");
        }

        if (options.AccessTokenLifetimeSeconds <= 0)
        {
            return ValidateOptionsResult.Fail("IdentityServer:AccessTokenLifetimeSeconds must be greater than zero.");
        }

        if (options.AbsoluteRefreshTokenLifetimeSeconds <= 0)
        {
            return ValidateOptionsResult.Fail("IdentityServer:AbsoluteRefreshTokenLifetimeSeconds must be greater than zero.");
        }

        if (options.SlidingRefreshTokenLifetimeSeconds <= 0)
        {
            return ValidateOptionsResult.Fail("IdentityServer:SlidingRefreshTokenLifetimeSeconds must be greater than zero.");
        }

        return ValidateOptionsResult.Success;
    }
}
