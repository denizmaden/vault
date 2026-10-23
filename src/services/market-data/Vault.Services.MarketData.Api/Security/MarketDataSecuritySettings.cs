namespace Vault.Services.MarketData.Api.Security;

public sealed class MarketDataSecuritySettings
{
    public const string SectionName = "MarketDataSecurity";

    public bool RequireAuthentication { get; init; }

    public bool AllowConnectionsWithoutOrigin { get; init; } = true;

    public IReadOnlyCollection<string> AllowedOrigins { get; init; } = [];

    public int RestRequestsPerMinute { get; init; } = 120;

    public int WebSocketHandshakesPerMinute { get; init; } = 20;
}
