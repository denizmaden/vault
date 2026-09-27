namespace Vault.Services.Identity.Api.Configuration;

public sealed class IdentityServerSettings
{
    public const string SectionName = "IdentityServer";

    public string SwaggerClientId { get; init; } = string.Empty;

    public string SwaggerClientSecret { get; init; } = string.Empty;

    public string? IssuerUri { get; init; }

    public int AccessTokenLifetimeSeconds { get; init; } = 3600;

    public int AbsoluteRefreshTokenLifetimeSeconds { get; init; } = 2_592_000;

    public int SlidingRefreshTokenLifetimeSeconds { get; init; } = 1_296_000;
}
