using Microsoft.Extensions.DependencyInjection;
using Vault.BuildingBlocks.Application;
using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Customer.Application.Features.CompleteCustomerOnboarding;
using Vault.Services.Customer.Application.Features.CreateCustomerProfile;
using Vault.Services.Customer.Application.Features.GetCustomerProfile;
using Vault.Services.Customer.Application.Features.UpdateCustomerProfile;
using Vault.Services.Customer.Application.Models;

namespace Vault.Services.Customer.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomerApplication(this IServiceCollection services)
    {
        services.AddBuildingBlocksApplication();

        services.AddScoped<ICommandHandler<CreateCustomerProfileCommand, Result<CustomerProfileCreated>>, CreateCustomerProfileCommandHandler>();
        services.AddScoped<IQueryHandler<GetCustomerProfileQuery, Result<CustomerProfileDetails>>, GetCustomerProfileQueryHandler>();
        services.AddScoped<ICommandHandler<UpdateCustomerProfileCommand, Result>, UpdateCustomerProfileCommandHandler>();
        services.AddScoped<ICommandHandler<CompleteCustomerOnboardingCommand, Result>, CompleteCustomerOnboardingCommandHandler>();

        services.AddScoped<IRequestValidator<CreateCustomerProfileCommand>, CreateCustomerProfileCommandValidator>();
        services.AddScoped<IRequestValidator<GetCustomerProfileQuery>, GetCustomerProfileQueryValidator>();
        services.AddScoped<IRequestValidator<UpdateCustomerProfileCommand>, UpdateCustomerProfileCommandValidator>();
        services.AddScoped<IRequestValidator<CompleteCustomerOnboardingCommand>, CompleteCustomerOnboardingCommandValidator>();

        return services;
    }
}
