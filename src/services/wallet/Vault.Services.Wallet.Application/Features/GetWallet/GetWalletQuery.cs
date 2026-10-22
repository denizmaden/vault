using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Wallet.Application.Models;

namespace Vault.Services.Wallet.Application.Features.GetWallet;

public sealed record GetWalletQuery(Guid WalletId) : IQuery<Result<WalletDetails>>;

