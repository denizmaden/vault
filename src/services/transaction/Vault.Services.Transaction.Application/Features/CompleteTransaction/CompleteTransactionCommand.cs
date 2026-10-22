using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Transaction.Application.Features.CompleteTransaction;

public sealed record CompleteTransactionCommand(Guid TransactionId) : ICommand<Result>;
