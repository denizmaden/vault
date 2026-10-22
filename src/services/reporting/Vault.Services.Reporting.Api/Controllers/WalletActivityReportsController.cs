using Microsoft.AspNetCore.Mvc;
using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Reporting.Api.Contracts;
using Vault.Services.Reporting.Application.Features.GetWalletActivityReport;
using Vault.Services.Reporting.Application.Features.GetWalletActivityReportsByOwner;
using Vault.Services.Reporting.Application.Models;

namespace Vault.Services.Reporting.Api.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class WalletActivityReportsController(IQueryDispatcher queryDispatcher) : ControllerBase
{
    [HttpGet("wallets/{walletId:guid}")]
    [ProducesResponseType<WalletActivityReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByWalletId(Guid walletId, CancellationToken cancellationToken)
    {
        var result = await queryDispatcher.DispatchAsync<GetWalletActivityReportQuery, Result<WalletActivityReportDetails>>(
            new GetWalletActivityReportQuery(walletId),
            cancellationToken);

        return result.IsSuccess
            ? Ok(ToResponse(result.Value))
            : Problem("Reporting operation failed.", result.Error.Description, statusCode: StatusCodes.Status404NotFound);
    }

    [HttpGet("owners/{ownerId:guid}/wallets")]
    [ProducesResponseType<IReadOnlyCollection<WalletActivityReportResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByOwnerId(Guid ownerId, CancellationToken cancellationToken)
    {
        var result = await queryDispatcher.DispatchAsync<GetWalletActivityReportsByOwnerQuery, Result<IReadOnlyCollection<WalletActivityReportDetails>>>(
            new GetWalletActivityReportsByOwnerQuery(ownerId),
            cancellationToken);

        return Ok(result.Value.Select(ToResponse));
    }

    private static WalletActivityReportResponse ToResponse(WalletActivityReportDetails details) =>
        new(
            details.WalletId,
            details.OwnerId,
            details.Currency,
            details.CurrentBalance,
            details.TotalDeposited,
            details.TotalWithdrawn,
            details.Version,
            details.OpenedOnUtc,
            details.LastUpdatedOnUtc,
            details.Entries.Select(x => new ActivityEntryResponse(
                x.Type,
                x.Amount,
                x.Reference,
                x.Version,
                x.OccurredOnUtc)).ToArray());
}
