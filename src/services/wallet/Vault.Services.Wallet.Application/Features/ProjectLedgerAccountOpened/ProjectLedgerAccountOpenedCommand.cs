using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Wallet.Application.Features.ProjectLedgerAccountOpened;

public sealed record ProjectLedgerAccountOpenedCommand(
    Guid WalletId,
    Guid OwnerId,
    string Currency,
    int Version,
    DateTime OccurredOnUtc) : ICommand<Result>;

