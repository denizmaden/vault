using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Ledger.Application.Models;

namespace Vault.Services.Ledger.Application.Features.OpenLedgerAccount;

public sealed record OpenLedgerAccountCommand(
    Guid LedgerAccountId,
    Guid OwnerId,
    string Currency) : ICommand<Result<LedgerAccountOpened>>;

