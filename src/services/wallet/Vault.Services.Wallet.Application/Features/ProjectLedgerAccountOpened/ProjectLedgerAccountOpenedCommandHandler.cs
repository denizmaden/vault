using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Wallet.Application.Abstractions.Caching;
using Vault.Services.Wallet.Application.Abstractions.Persistence;
using Vault.Services.Wallet.Domain.Entities;
using Vault.Services.Wallet.Domain.Errors;
using Vault.Services.Wallet.Domain.ValueObjects;

namespace Vault.Services.Wallet.Application.Features.ProjectLedgerAccountOpened;

public sealed class ProjectLedgerAccountOpenedCommandHandler(
    IWalletProjectionRepository repository,
    IWalletProjectionCache cache) : ICommandHandler<ProjectLedgerAccountOpenedCommand, Result>
{
    public async Task<Result> Handle(ProjectLedgerAccountOpenedCommand command, CancellationToken cancellationToken = default)
    {
        if (await repository.ExistsAsync(command.WalletId, cancellationToken))
        {
            return Result.Failure(WalletErrors.WalletAlreadyExists);
        }

        var currencyResult = WalletCurrency.Create(command.Currency);

        if (currencyResult.IsFailure)
        {
            return Result.Failure(currencyResult.Error);
        }

        var walletResult = Wallet.Domain.Entities.Wallet.Open(
            command.WalletId,
            command.OwnerId,
            currencyResult.Value,
            command.Version,
            command.OccurredOnUtc);

        if (walletResult.IsFailure)
        {
            return Result.Failure(walletResult.Error);
        }

        await repository.UpsertAsync(walletResult.Value, cancellationToken);
        await cache.RemoveAsync(command.WalletId, cancellationToken);
        return Result.Success();
    }
}

