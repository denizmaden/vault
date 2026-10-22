using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Reporting.Application.Models;

namespace Vault.Services.Reporting.Application.Features.GetWalletActivityReportsByOwner;

public sealed record GetWalletActivityReportsByOwnerQuery(Guid OwnerId) : IQuery<Result<IReadOnlyCollection<WalletActivityReportDetails>>>;
