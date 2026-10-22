using Microsoft.AspNetCore.Mvc;
using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Wallet.Api.Contracts;
using Vault.Services.Wallet.Application.Features.GetWallet;
using Vault.Services.Wallet.Application.Features.GetWalletsByOwner;
using Vault.Services.Wallet.Application.Models;

namespace Vault.Services.Wallet.Api.Controllers;

[ApiController]
[Route("api/wallets")]
public sealed class WalletsController(IQueryDispatcher queryDispatcher) : ControllerBase
{
    [HttpGet("{walletId:guid}")]
    [ProducesResponseType<WalletResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid walletId, CancellationToken cancellationToken)
    {
        var result = await queryDispatcher.DispatchAsync<GetWalletQuery, Result<WalletDetails>>(
            new GetWalletQuery(walletId),
            cancellationToken);

        return result.IsSuccess
            ? Ok(ToResponse(result.Value))
            : Problem("Wallet operation failed.", result.Error.Description, statusCode: StatusCodes.Status404NotFound);
    }

    [HttpGet("owners/{ownerId:guid}")]
    [ProducesResponseType<IReadOnlyCollection<WalletResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByOwner(Guid ownerId, CancellationToken cancellationToken)
    {
        var result = await queryDispatcher.DispatchAsync<GetWalletsByOwnerQuery, Result<IReadOnlyCollection<WalletDetails>>>(
            new GetWalletsByOwnerQuery(ownerId),
            cancellationToken);

        return Ok(result.Value.Select(ToResponse));
    }

    private static WalletResponse ToResponse(WalletDetails details) =>
        new(
            details.WalletId,
            details.OwnerId,
            details.Currency,
            details.Balance,
            details.Status,
            details.Version,
            details.OpenedOnUtc,
            details.LastSyncedOnUtc);
}

