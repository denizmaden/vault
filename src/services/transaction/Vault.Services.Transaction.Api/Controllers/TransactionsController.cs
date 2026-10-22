using Microsoft.AspNetCore.Mvc;
using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Transaction.Api.Contracts;
using Vault.Services.Transaction.Application.Features.CompleteTransaction;
using Vault.Services.Transaction.Application.Features.CreateTransferTransaction;
using Vault.Services.Transaction.Application.Features.FailTransaction;
using Vault.Services.Transaction.Application.Features.GetTransaction;
using Vault.Services.Transaction.Application.Features.GetTransactionsByCustomer;
using Vault.Services.Transaction.Application.Features.ReserveTransactionFunds;
using Vault.Services.Transaction.Application.Models;

namespace Vault.Services.Transaction.Api.Controllers;

[ApiController]
[Route("api/transactions")]
public sealed class TransactionsController(
    ICommandDispatcher commandDispatcher,
    IQueryDispatcher queryDispatcher) : ControllerBase
{
    [HttpPost("transfers")]
    [ProducesResponseType<CreateTransferTransactionResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateTransfer(
        [FromBody] CreateTransferTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.DispatchAsync<CreateTransferTransactionCommand, Result<TransactionCreated>>(
            new CreateTransferTransactionCommand(
                request.TransactionId,
                request.InitiatingCustomerId,
                request.SourceWalletId,
                request.DestinationWalletId,
                request.Currency,
                request.Amount,
                request.Reference),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { transactionId = result.Value.TransactionId }, new CreateTransferTransactionResponse(result.Value.TransactionId, result.Value.Status))
            : ToFailureResult(result);
    }

    [HttpPost("{transactionId:guid}/reserve-funds")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReserveFunds(Guid transactionId, CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.DispatchAsync<ReserveTransactionFundsCommand, Result>(
            new ReserveTransactionFundsCommand(transactionId),
            cancellationToken);

        return result.IsSuccess ? NoContent() : ToFailureResult(result);
    }

    [HttpPost("{transactionId:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(Guid transactionId, CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.DispatchAsync<CompleteTransactionCommand, Result>(
            new CompleteTransactionCommand(transactionId),
            cancellationToken);

        return result.IsSuccess ? NoContent() : ToFailureResult(result);
    }

    [HttpPost("{transactionId:guid}/fail")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Fail(
        Guid transactionId,
        [FromBody] FailTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.DispatchAsync<FailTransactionCommand, Result>(
            new FailTransactionCommand(transactionId, request.FailureReason),
            cancellationToken);

        return result.IsSuccess ? NoContent() : ToFailureResult(result);
    }

    [HttpGet("{transactionId:guid}")]
    [ProducesResponseType<TransactionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid transactionId, CancellationToken cancellationToken)
    {
        var result = await queryDispatcher.DispatchAsync<GetTransactionQuery, Result<TransactionDetails>>(
            new GetTransactionQuery(transactionId),
            cancellationToken);

        return result.IsSuccess ? Ok(ToResponse(result.Value)) : ToFailureResult(result);
    }

    [HttpGet("customers/{customerId:guid}")]
    [ProducesResponseType<IReadOnlyCollection<TransactionSummaryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await queryDispatcher.DispatchAsync<GetTransactionsByCustomerQuery, Result<IReadOnlyCollection<TransactionSummary>>>(
            new GetTransactionsByCustomerQuery(customerId),
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value.Select(ToResponse))
            : ToFailureResult(result);
    }

    private IActionResult ToFailureResult(Result<TransactionCreated> result) => ToFailureResult((Result)result);

    private IActionResult ToFailureResult(Result<TransactionDetails> result) => ToFailureResult((Result)result);

    private IActionResult ToFailureResult(Result<IReadOnlyCollection<TransactionSummary>> result) => ToFailureResult((Result)result);

    private IActionResult ToFailureResult(Result result)
    {
        var statusCode = result.Error.Code switch
        {
            "transaction.not_found" => StatusCodes.Status404NotFound,
            "transaction.duplicate_transaction" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return Problem("Transaction operation failed.", result.Error.Description, statusCode: statusCode);
    }

    private static TransactionResponse ToResponse(TransactionDetails details) =>
        new(
            details.TransactionId,
            details.InitiatingCustomerId,
            details.SourceWalletId,
            details.DestinationWalletId,
            details.Currency,
            details.Amount,
            details.Reference,
            details.Type,
            details.Status,
            details.FailureReason,
            details.CreatedOnUtc,
            details.LastUpdatedOnUtc,
            details.FundsReservedOnUtc,
            details.CompletedOnUtc,
            details.FailedOnUtc);

    private static TransactionSummaryResponse ToResponse(TransactionSummary summary) =>
        new(
            summary.TransactionId,
            summary.InitiatingCustomerId,
            summary.SourceWalletId,
            summary.DestinationWalletId,
            summary.Currency,
            summary.Amount,
            summary.Status,
            summary.CreatedOnUtc,
            summary.LastUpdatedOnUtc);
}
