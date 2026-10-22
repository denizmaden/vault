using Microsoft.AspNetCore.Mvc;
using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Ledger.Api.Contracts;
using Vault.Services.Ledger.Application.Features.DepositFunds;
using Vault.Services.Ledger.Application.Features.GetLedgerAccount;
using Vault.Services.Ledger.Application.Features.GetLedgerAccountEvents;
using Vault.Services.Ledger.Application.Features.OpenLedgerAccount;
using Vault.Services.Ledger.Application.Features.WithdrawFunds;
using Vault.Services.Ledger.Application.Models;

namespace Vault.Services.Ledger.Api.Controllers;

[ApiController]
[Route("api/ledger-accounts")]
public sealed class LedgerAccountsController(
    ICommandDispatcher commandDispatcher,
    IQueryDispatcher queryDispatcher) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<OpenLedgerAccountResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Open(
        [FromBody] OpenLedgerAccountRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.DispatchAsync<OpenLedgerAccountCommand, Result<LedgerAccountOpened>>(
            new OpenLedgerAccountCommand(request.LedgerAccountId, request.OwnerId, request.Currency),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { ledgerAccountId = result.Value.LedgerAccountId }, ToResponse(result.Value))
            : ToFailureResult(result);
    }

    [HttpPost("{ledgerAccountId:guid}/deposit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deposit(
        Guid ledgerAccountId,
        [FromBody] MoneyMovementRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.DispatchAsync<DepositFundsCommand, Result>(
            new DepositFundsCommand(ledgerAccountId, request.Amount, request.Reference),
            cancellationToken);

        return result.IsSuccess ? NoContent() : ToFailureResult(result);
    }

    [HttpPost("{ledgerAccountId:guid}/withdraw")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Withdraw(
        Guid ledgerAccountId,
        [FromBody] MoneyMovementRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.DispatchAsync<WithdrawFundsCommand, Result>(
            new WithdrawFundsCommand(ledgerAccountId, request.Amount, request.Reference),
            cancellationToken);

        return result.IsSuccess ? NoContent() : ToFailureResult(result);
    }

    [HttpGet("{ledgerAccountId:guid}")]
    [ProducesResponseType<LedgerAccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid ledgerAccountId, CancellationToken cancellationToken)
    {
        var result = await queryDispatcher.DispatchAsync<GetLedgerAccountQuery, Result<LedgerAccountDetails>>(
            new GetLedgerAccountQuery(ledgerAccountId),
            cancellationToken);

        return result.IsSuccess
            ? Ok(ToResponse(result.Value))
            : ToFailureResult(result);
    }

    [HttpGet("{ledgerAccountId:guid}/events")]
    [ProducesResponseType<IReadOnlyCollection<LedgerEventResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEvents(Guid ledgerAccountId, CancellationToken cancellationToken)
    {
        var result = await queryDispatcher.DispatchAsync<GetLedgerAccountEventsQuery, Result<IReadOnlyCollection<LedgerEventDetails>>>(
            new GetLedgerAccountEventsQuery(ledgerAccountId),
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value.Select(ToResponse))
            : ToFailureResult(result);
    }

    private IActionResult ToFailureResult(Result<LedgerAccountOpened> result) => ToFailureResult((Result)result);

    private IActionResult ToFailureResult(Result<LedgerAccountDetails> result) => ToFailureResult((Result)result);

    private IActionResult ToFailureResult(Result<IReadOnlyCollection<LedgerEventDetails>> result) => ToFailureResult((Result)result);

    private IActionResult ToFailureResult(Result result)
    {
        var statusCode = result.Error.Code switch
        {
            "ledger.not_found" => StatusCodes.Status404NotFound,
            "ledger.duplicate_account" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return Problem("Ledger operation failed.", result.Error.Description, statusCode: statusCode);
    }

    private static OpenLedgerAccountResponse ToResponse(LedgerAccountOpened opened) =>
        new(opened.LedgerAccountId, opened.OwnerId, opened.Currency, opened.Status);

    private static LedgerAccountResponse ToResponse(LedgerAccountDetails details) =>
        new(
            details.LedgerAccountId,
            details.OwnerId,
            details.Currency,
            details.Balance,
            details.Status,
            details.Version,
            details.OpenedOnUtc);

    private static LedgerEventResponse ToResponse(LedgerEventDetails details) =>
        new(details.EventId, details.EventType, details.Version, details.OccurredOnUtc, details.Data);
}

