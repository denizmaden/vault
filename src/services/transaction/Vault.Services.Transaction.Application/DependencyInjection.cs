using Microsoft.Extensions.DependencyInjection;
using Vault.BuildingBlocks.Application;
using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Transaction.Application.Features.CompleteTransaction;
using Vault.Services.Transaction.Application.Features.CreateTransferTransaction;
using Vault.Services.Transaction.Application.Features.FailTransaction;
using Vault.Services.Transaction.Application.Features.GetTransaction;
using Vault.Services.Transaction.Application.Features.GetTransactionsByCustomer;
using Vault.Services.Transaction.Application.Features.ReserveTransactionFunds;
using Vault.Services.Transaction.Application.Models;

namespace Vault.Services.Transaction.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTransactionApplication(this IServiceCollection services)
    {
        services.AddBuildingBlocksApplication();

        services.AddScoped<ICommandHandler<CreateTransferTransactionCommand, Result<TransactionCreated>>, CreateTransferTransactionCommandHandler>();
        services.AddScoped<ICommandHandler<ReserveTransactionFundsCommand, Result>, ReserveTransactionFundsCommandHandler>();
        services.AddScoped<ICommandHandler<CompleteTransactionCommand, Result>, CompleteTransactionCommandHandler>();
        services.AddScoped<ICommandHandler<FailTransactionCommand, Result>, FailTransactionCommandHandler>();
        services.AddScoped<IQueryHandler<GetTransactionQuery, Result<TransactionDetails>>, GetTransactionQueryHandler>();
        services.AddScoped<IQueryHandler<GetTransactionsByCustomerQuery, Result<IReadOnlyCollection<TransactionSummary>>>, GetTransactionsByCustomerQueryHandler>();

        services.AddScoped<IRequestValidator<CreateTransferTransactionCommand>, CreateTransferTransactionCommandValidator>();
        services.AddScoped<IRequestValidator<ReserveTransactionFundsCommand>, ReserveTransactionFundsCommandValidator>();
        services.AddScoped<IRequestValidator<CompleteTransactionCommand>, CompleteTransactionCommandValidator>();
        services.AddScoped<IRequestValidator<FailTransactionCommand>, FailTransactionCommandValidator>();
        services.AddScoped<IRequestValidator<GetTransactionQuery>, GetTransactionQueryValidator>();
        services.AddScoped<IRequestValidator<GetTransactionsByCustomerQuery>, GetTransactionsByCustomerQueryValidator>();

        return services;
    }
}
