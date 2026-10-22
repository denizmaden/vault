using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Transaction.Application.Features.FailTransaction;

public sealed record FailTransactionCommand(Guid TransactionId, string? FailureReason) : ICommand<Result>;
