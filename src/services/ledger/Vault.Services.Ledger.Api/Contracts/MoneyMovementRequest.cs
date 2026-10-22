namespace Vault.Services.Ledger.Api.Contracts;

public sealed record MoneyMovementRequest(
    decimal Amount,
    string Reference);

