using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Transaction.Application.Models;

namespace Vault.Services.Transaction.Application.Features.CreateTransferTransaction;

public sealed record CreateTransferTransactionCommand(
    Guid TransactionId,
    Guid InitiatingCustomerId,
    Guid SourceWalletId,
    Guid DestinationWalletId,
    string Currency,
    decimal Amount,
    string? Reference) : ICommand<Result<TransactionCreated>>;
