using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Vault.Services.Identity.Api.Configuration;

namespace Vault.Services.Identity.Api.IdentityServer;

public static class IdentityServerConfiguration
{
    public static IEnumerable<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile()
    ];

    public static IEnumerable<ApiResource> ApiResources =>
    [
        new(IdentityServerConstants.LocalApi.ScopeName, "Vault Identity Local API")
        {
            Scopes = { IdentityServerConstants.LocalApi.ScopeName }
        },
        new("vault.identity", "Vault Identity API")
        {
            Scopes = { "vault.identity.api" }
        },
        new("vault.customer", "Vault Customer API")
        {
            Scopes = { "vault.customer.api" }
        },
        new("vault.ledger", "Vault Ledger API")
        {
            Scopes = { "vault.ledger.api" }
        },
        new("vault.wallet", "Vault Wallet API")
        {
            Scopes = { "vault.wallet.api" }
        },
        new("vault.transaction", "Vault Transaction API")
        {
            Scopes = { "vault.transaction.api" }
        },
        new("vault.reporting", "Vault Reporting API")
        {
            Scopes = { "vault.reporting.api" }
        }
    ];

    public static IEnumerable<ApiScope> ApiScopes =>
    [
        new(IdentityServerConstants.LocalApi.ScopeName, "Vault Identity Local API"),
        new("vault.identity.api", "Vault Identity API"),
        new("vault.customer.api", "Vault Customer API"),
        new("vault.ledger.api", "Vault Ledger API"),
        new("vault.wallet.api", "Vault Wallet API"),
        new("vault.transaction.api", "Vault Transaction API"),
        new("vault.reporting.api", "Vault Reporting API")
    ];

    public static IEnumerable<Client> CreateClients(IdentityServerSettings settings) =>
    [
        new()
        {
            ClientId = settings.SwaggerClientId,
            ClientName = "Vault Swagger Client",
            AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
            ClientSecrets = { new Secret(settings.SwaggerClientSecret.Sha256()) },
            AllowedScopes =
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                IdentityServerConstants.StandardScopes.OfflineAccess,
                IdentityServerConstants.LocalApi.ScopeName,
                "vault.identity.api",
                "vault.customer.api",
                "vault.ledger.api",
                "vault.wallet.api",
                "vault.transaction.api",
                "vault.reporting.api"
            },
            AllowOfflineAccess = true,
            AccessTokenLifetime = settings.AccessTokenLifetimeSeconds,
            IdentityTokenLifetime = 300,
            AuthorizationCodeLifetime = 300,
            AbsoluteRefreshTokenLifetime = settings.AbsoluteRefreshTokenLifetimeSeconds,
            SlidingRefreshTokenLifetime = settings.SlidingRefreshTokenLifetimeSeconds,
            RefreshTokenExpiration = TokenExpiration.Sliding,
            RefreshTokenUsage = TokenUsage.OneTimeOnly,
            UpdateAccessTokenClaimsOnRefresh = true
        }
    ];
}
