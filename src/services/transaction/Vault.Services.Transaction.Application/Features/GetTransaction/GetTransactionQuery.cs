using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Transaction.Application.Models;

namespace Vault.Services.Transaction.Application.Features.GetTransaction;

public sealed record GetTransactionQuery(Guid TransactionId) : IQuery<Result<TransactionDetails>>;
