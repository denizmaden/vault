using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Ledger.Application.Abstractions.Persistence;
using Vault.Services.Ledger.Domain.Errors;
using Vault.Services.Ledger.Domain.ValueObjects;

namespace Vault.Services.Ledger.Application.Features.DepositFunds;

public sealed class DepositFundsCommandHandler(
    ILedgerAccountRepository repository) : ICommandHandler<DepositFundsCommand, Result>
{
    public async Task<Result> Handle(
        DepositFundsCommand command,
        CancellationToken cancellationToken = default)
    {
        var account = await repository.LoadAsync(command.LedgerAccountId, cancellationToken);

        if (account is null)
        {
            return Result.Failure(LedgerErrors.LedgerAccountNotFound);
        }

        var amountResult = MoneyAmount.Create(command.Amount);

        if (amountResult.IsFailure)
        {
            return Result.Failure(amountResult.Error);
        }

        var depositResult = account.Deposit(amountResult.Value, command.Reference, DateTime.UtcNow);

        if (depositResult.IsFailure)
        {
            return depositResult;
        }

        await repository.SaveAsync(account, cancellationToken);

        return Result.Success();
    }
}

