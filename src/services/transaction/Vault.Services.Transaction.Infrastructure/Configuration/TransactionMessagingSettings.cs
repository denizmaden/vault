namespace Vault.Services.Transaction.Infrastructure.Configuration;

public sealed class TransactionMessagingSettings
{
    public const string SectionName = "TransactionMessaging";

    public string RequestedQueue { get; init; } = "vault.transaction.transfer.requested";

    public string ReservedQueue { get; init; } = "vault.transaction.transfer.funds-reserved";

    public string CompletedQueue { get; init; } = "vault.transaction.transfer.completed";

    public string FailedQueue { get; init; } = "vault.transaction.transfer.failed";
}
