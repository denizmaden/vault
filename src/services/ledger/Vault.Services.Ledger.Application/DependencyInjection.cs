using Microsoft.Extensions.DependencyInjection;
using Vault.BuildingBlocks.Application;
using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Ledger.Application.Features.DepositFunds;
using Vault.Services.Ledger.Application.Features.GetLedgerAccount;
using Vault.Services.Ledger.Application.Features.GetLedgerAccountEvents;
using Vault.Services.Ledger.Application.Features.OpenLedgerAccount;
using Vault.Services.Ledger.Application.Features.WithdrawFunds;
using Vault.Services.Ledger.Application.Models;

namespace Vault.Services.Ledger.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddLedgerApplication(this IServiceCollection services)
    {
        services.AddBuildingBlocksApplication();

        services.AddScoped<ICommandHandler<OpenLedgerAccountCommand, Result<LedgerAccountOpened>>, OpenLedgerAccountCommandHandler>();
        services.AddScoped<ICommandHandler<DepositFundsCommand, Result>, DepositFundsCommandHandler>();
        services.AddScoped<ICommandHandler<WithdrawFundsCommand, Result>, WithdrawFundsCommandHandler>();
        services.AddScoped<IQueryHandler<GetLedgerAccountQuery, Result<LedgerAccountDetails>>, GetLedgerAccountQueryHandler>();
        services.AddScoped<IQueryHandler<GetLedgerAccountEventsQuery, Result<IReadOnlyCollection<LedgerEventDetails>>>, GetLedgerAccountEventsQueryHandler>();

        services.AddScoped<IRequestValidator<OpenLedgerAccountCommand>, OpenLedgerAccountCommandValidator>();
        services.AddScoped<IRequestValidator<DepositFundsCommand>, DepositFundsCommandValidator>();
        services.AddScoped<IRequestValidator<WithdrawFundsCommand>, WithdrawFundsCommandValidator>();
        services.AddScoped<IRequestValidator<GetLedgerAccountQuery>, GetLedgerAccountQueryValidator>();
        services.AddScoped<IRequestValidator<GetLedgerAccountEventsQuery>, GetLedgerAccountEventsQueryValidator>();

        return services;
    }
}

