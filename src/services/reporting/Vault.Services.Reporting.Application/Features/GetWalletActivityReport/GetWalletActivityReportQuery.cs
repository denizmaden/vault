using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Reporting.Application.Models;

namespace Vault.Services.Reporting.Application.Features.GetWalletActivityReport;

public sealed record GetWalletActivityReportQuery(Guid WalletId) : IQuery<Result<WalletActivityReportDetails>>;
