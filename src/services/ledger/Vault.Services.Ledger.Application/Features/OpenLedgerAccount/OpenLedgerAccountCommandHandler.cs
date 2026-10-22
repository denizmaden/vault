using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Ledger.Application.Abstractions.Persistence;
using Vault.Services.Ledger.Application.Models;
using Vault.Services.Ledger.Domain.Entities;
using Vault.Services.Ledger.Domain.Errors;
using Vault.Services.Ledger.Domain.ValueObjects;

namespace Vault.Services.Ledger.Application.Features.OpenLedgerAccount;

public sealed class OpenLedgerAccountCommandHandler(
    ILedgerAccountRepository repository) : ICommandHandler<OpenLedgerAccountCommand, Result<LedgerAccountOpened>>
{
    public async Task<Result<LedgerAccountOpened>> Handle(
        OpenLedgerAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        if (await repository.ExistsAsync(command.LedgerAccountId, cancellationToken))
        {
            return Result<LedgerAccountOpened>.Failure(LedgerErrors.DuplicateLedgerAccount);
        }

        var currencyResult = Currency.Create(command.Currency);

        if (currencyResult.IsFailure)
        {
            return Result<LedgerAccountOpened>.Failure(currencyResult.Error);
        }

        var accountResult = LedgerAccount.Open(
            command.LedgerAccountId,
            command.OwnerId,
            currencyResult.Value,
            DateTime.UtcNow);

        if (accountResult.IsFailure)
        {
            return Result<LedgerAccountOpened>.Failure(accountResult.Error);
        }

        await repository.SaveAsync(accountResult.Value, cancellationToken);

        return Result<LedgerAccountOpened>.Success(
            new LedgerAccountOpened(
                accountResult.Value.Id,
                accountResult.Value.OwnerId,
                accountResult.Value.Currency.Code,
                accountResult.Value.Status.ToString()));
    }
}

