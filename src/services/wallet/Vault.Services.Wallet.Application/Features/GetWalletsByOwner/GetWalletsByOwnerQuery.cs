using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Wallet.Application.Models;

namespace Vault.Services.Wallet.Application.Features.GetWalletsByOwner;

public sealed record GetWalletsByOwnerQuery(Guid OwnerId) : IQuery<Result<IReadOnlyCollection<WalletDetails>>>;

