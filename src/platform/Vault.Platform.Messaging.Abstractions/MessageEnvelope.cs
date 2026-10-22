namespace Vault.Platform.Messaging.Abstractions;

public sealed record MessageEnvelope(
    string Destination,
    string Key,
    string Type,
    string Payload,
    DateTime OccurredOnUtc,
    IReadOnlyDictionary<string, string>? Headers = null);
