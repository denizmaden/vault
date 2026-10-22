using Vault.BuildingBlocks.Domain.Abstractions;

namespace Vault.Services.Ledger.Domain.Events;

public abstract record LedgerAccountEvent(Guid LedgerAccountId, DateTime OccurredOnUtc) : IDomainEvent;

