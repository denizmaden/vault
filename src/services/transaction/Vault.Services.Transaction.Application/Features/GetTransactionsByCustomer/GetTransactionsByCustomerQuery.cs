using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Transaction.Application.Models;

namespace Vault.Services.Transaction.Application.Features.GetTransactionsByCustomer;

public sealed record GetTransactionsByCustomerQuery(Guid CustomerId) : IQuery<Result<IReadOnlyCollection<TransactionSummary>>>;
