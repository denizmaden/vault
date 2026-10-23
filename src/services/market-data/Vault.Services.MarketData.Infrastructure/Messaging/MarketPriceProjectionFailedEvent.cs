namespace Vault.Services.MarketData.Infrastructure.Messaging;

public sealed record MarketPriceProjectionFailedEvent(
    string OriginalDestination,
    string OriginalKey,
    string OriginalType,
    string OriginalPayload,
    DateTime OriginalOccurredOnUtc,
    IReadOnlyDictionary<string, string>? OriginalHeaders,
    int ProcessingAttempts,
    string ErrorType,
    string ErrorMessage,
    DateTime FailedOnUtc);
