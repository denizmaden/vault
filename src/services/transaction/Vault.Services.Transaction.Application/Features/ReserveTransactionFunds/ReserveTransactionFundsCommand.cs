using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Transaction.Application.Features.ReserveTransactionFunds;

public sealed record ReserveTransactionFundsCommand(Guid TransactionId) : ICommand<Result>;
