using Microsoft.Extensions.DependencyInjection;
using Vault.BuildingBlocks.Application;
using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Wallet.Application.Features.GetWallet;
using Vault.Services.Wallet.Application.Features.GetWalletsByOwner;
using Vault.Services.Wallet.Application.Features.ProjectFundsDeposited;
using Vault.Services.Wallet.Application.Features.ProjectFundsWithdrawn;
using Vault.Services.Wallet.Application.Features.ProjectLedgerAccountOpened;
using Vault.Services.Wallet.Application.Models;

namespace Vault.Services.Wallet.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddWalletApplication(this IServiceCollection services)
    {
        services.AddBuildingBlocksApplication();

        services.AddScoped<ICommandHandler<ProjectLedgerAccountOpenedCommand, Result>, ProjectLedgerAccountOpenedCommandHandler>();
        services.AddScoped<ICommandHandler<ProjectFundsDepositedCommand, Result>, ProjectFundsDepositedCommandHandler>();
        services.AddScoped<ICommandHandler<ProjectFundsWithdrawnCommand, Result>, ProjectFundsWithdrawnCommandHandler>();
        services.AddScoped<IQueryHandler<GetWalletQuery, Result<WalletDetails>>, GetWalletQueryHandler>();
        services.AddScoped<IQueryHandler<GetWalletsByOwnerQuery, Result<IReadOnlyCollection<WalletDetails>>>, GetWalletsByOwnerQueryHandler>();

        services.AddScoped<IRequestValidator<ProjectLedgerAccountOpenedCommand>, ProjectLedgerAccountOpenedCommandValidator>();
        services.AddScoped<IRequestValidator<ProjectFundsDepositedCommand>, ProjectFundsDepositedCommandValidator>();
        services.AddScoped<IRequestValidator<ProjectFundsWithdrawnCommand>, ProjectFundsWithdrawnCommandValidator>();
        services.AddScoped<IRequestValidator<GetWalletQuery>, GetWalletQueryValidator>();
        services.AddScoped<IRequestValidator<GetWalletsByOwnerQuery>, GetWalletsByOwnerQueryValidator>();

        return services;
    }
}

