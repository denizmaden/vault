using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Ledger.Application.Abstractions.Persistence;
using Vault.Services.Ledger.Application.Models;
using Vault.Services.Ledger.Domain.Errors;

namespace Vault.Services.Ledger.Application.Features.GetLedgerAccount;

public sealed class GetLedgerAccountQueryHandler(
    ILedgerReadRepository readRepository) : IQueryHandler<GetLedgerAccountQuery, Result<LedgerAccountDetails>>
{
    public async Task<Result<LedgerAccountDetails>> Handle(
        GetLedgerAccountQuery query,
        CancellationToken cancellationToken = default)
    {
        var account = await readRepository.GetAccountAsync(query.LedgerAccountId, cancellationToken);

        return account is null
            ? Result<LedgerAccountDetails>.Failure(LedgerErrors.LedgerAccountNotFound)
            : Result<LedgerAccountDetails>.Success(account);
    }
}

