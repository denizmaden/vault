using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Wallet.Application.Abstractions.Caching;
using Vault.Services.Wallet.Application.Abstractions.Persistence;
using Vault.Services.Wallet.Domain.Errors;
using Vault.Services.Wallet.Domain.ValueObjects;

namespace Vault.Services.Wallet.Application.Features.ProjectFundsWithdrawn;

public sealed class ProjectFundsWithdrawnCommandHandler(
    IWalletProjectionRepository repository,
    IWalletProjectionCache cache) : ICommandHandler<ProjectFundsWithdrawnCommand, Result>
{
    public async Task<Result> Handle(ProjectFundsWithdrawnCommand command, CancellationToken cancellationToken = default)
    {
        var wallet = await repository.GetAsync(command.WalletId, cancellationToken);

        if (wallet is null)
        {
            return Result.Failure(WalletErrors.WalletNotFound);
        }

        var amountResult = WalletAmount.Create(command.Amount);

        if (amountResult.IsFailure)
        {
            return Result.Failure(amountResult.Error);
        }

        var applyResult = wallet.ApplyWithdrawal(amountResult.Value, command.Version, command.OccurredOnUtc);

        if (applyResult.IsFailure)
        {
            return applyResult;
        }

        await repository.UpsertAsync(wallet, cancellationToken);
        await cache.RemoveAsync(command.WalletId, cancellationToken);
        return Result.Success();
    }
}

