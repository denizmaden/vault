using Microsoft.AspNetCore.Mvc;
using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Wallet.Api.Contracts;
using Vault.Services.Wallet.Application.Features.ProjectFundsDeposited;
using Vault.Services.Wallet.Application.Features.ProjectFundsWithdrawn;
using Vault.Services.Wallet.Application.Features.ProjectLedgerAccountOpened;

namespace Vault.Services.Wallet.Api.Controllers;

[ApiController]
[Route("api/wallet-projections/ledger-events")]
public sealed class WalletProjectionController(ICommandDispatcher commandDispatcher) : ControllerBase
{
    [HttpPost("opened")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ProjectOpened(
        [FromBody] ProjectLedgerAccountOpenedRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.DispatchAsync<ProjectLedgerAccountOpenedCommand, Result>(
            new ProjectLedgerAccountOpenedCommand(
                request.WalletId,
                request.OwnerId,
                request.Currency,
                request.Version,
                request.OccurredOnUtc),
            cancellationToken);

        return result.IsSuccess ? NoContent() : ToFailureResult(result);
    }

    [HttpPost("deposited")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProjectDeposited(
        [FromBody] ProjectMoneyMovementRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.DispatchAsync<ProjectFundsDepositedCommand, Result>(
            new ProjectFundsDepositedCommand(
                request.WalletId,
                request.Amount,
                request.Version,
                request.OccurredOnUtc),
            cancellationToken);

        return result.IsSuccess ? NoContent() : ToFailureResult(result);
    }

    [HttpPost("withdrawn")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProjectWithdrawn(
        [FromBody] ProjectMoneyMovementRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.DispatchAsync<ProjectFundsWithdrawnCommand, Result>(
            new ProjectFundsWithdrawnCommand(
                request.WalletId,
                request.Amount,
                request.Version,
                request.OccurredOnUtc),
            cancellationToken);

        return result.IsSuccess ? NoContent() : ToFailureResult(result);
    }

    private IActionResult ToFailureResult(Result result)
    {
        var statusCode = result.Error.Code switch
        {
            "wallet.not_found" => StatusCodes.Status404NotFound,
            "wallet.duplicate_wallet" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return Problem("Wallet projection operation failed.", result.Error.Description, statusCode: statusCode);
    }
}

