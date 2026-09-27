namespace Vault.BuildingBlocks.Api.Authentication;

public sealed class VaultApiAuthenticationSettings
{
    public string Authority { get; init; } = string.Empty;

    public string? MetadataAddress { get; init; }

    public bool RequireHttpsMetadata { get; init; }
}
