using Microsoft.Extensions.DependencyInjection;
using Vault.BuildingBlocks.Application;
using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Reporting.Application.Features.GetWalletActivityReport;
using Vault.Services.Reporting.Application.Features.GetWalletActivityReportsByOwner;
using Vault.Services.Reporting.Application.Features.ProjectFundsDeposited;
using Vault.Services.Reporting.Application.Features.ProjectFundsWithdrawn;
using Vault.Services.Reporting.Application.Features.ProjectLedgerAccountOpened;
using Vault.Services.Reporting.Application.Models;

namespace Vault.Services.Reporting.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddReportingApplication(this IServiceCollection services)
    {
        services.AddBuildingBlocksApplication();

        services.AddScoped<ICommandHandler<ProjectLedgerAccountOpenedToReportCommand, Result>, ProjectLedgerAccountOpenedToReportCommandHandler>();
        services.AddScoped<ICommandHandler<ProjectFundsDepositedToReportCommand, Result>, ProjectFundsDepositedToReportCommandHandler>();
        services.AddScoped<ICommandHandler<ProjectFundsWithdrawnToReportCommand, Result>, ProjectFundsWithdrawnToReportCommandHandler>();
        services.AddScoped<IQueryHandler<GetWalletActivityReportQuery, Result<WalletActivityReportDetails>>, GetWalletActivityReportQueryHandler>();
        services.AddScoped<IQueryHandler<GetWalletActivityReportsByOwnerQuery, Result<IReadOnlyCollection<WalletActivityReportDetails>>>, GetWalletActivityReportsByOwnerQueryHandler>();

        services.AddScoped<IRequestValidator<ProjectLedgerAccountOpenedToReportCommand>, ProjectLedgerAccountOpenedToReportCommandValidator>();
        services.AddScoped<IRequestValidator<ProjectFundsDepositedToReportCommand>, ProjectFundsDepositedToReportCommandValidator>();
        services.AddScoped<IRequestValidator<ProjectFundsWithdrawnToReportCommand>, ProjectFundsWithdrawnToReportCommandValidator>();
        services.AddScoped<IRequestValidator<GetWalletActivityReportQuery>, GetWalletActivityReportQueryValidator>();
        services.AddScoped<IRequestValidator<GetWalletActivityReportsByOwnerQuery>, GetWalletActivityReportsByOwnerQueryValidator>();

        return services;
    }
}
