using Microsoft.Extensions.DependencyInjection;
using Vault.BuildingBlocks.Application;
using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Application.Features.AssignClaim;
using Vault.Services.Identity.Application.Features.AssignRole;
using Vault.Services.Identity.Application.Features.ChangePassword;
using Vault.Services.Identity.Application.Features.ConfirmEmail;
using Vault.Services.Identity.Application.Features.CreateRole;
using Vault.Services.Identity.Application.Features.GenerateEmailConfirmationToken;
using Vault.Services.Identity.Application.Features.GeneratePasswordResetToken;
using Vault.Services.Identity.Application.Features.GetIdentityUser;
using Vault.Services.Identity.Application.Features.RegisterIdentityUser;
using Vault.Services.Identity.Application.Features.ResetPassword;

namespace Vault.Services.Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        services.AddBuildingBlocksApplication();

        services.AddScoped<ICommandHandler<RegisterIdentityUserCommand, Result<RegisterIdentityUserResponse>>, RegisterIdentityUserCommandHandler>();
        services.AddScoped<IQueryHandler<GetIdentityUserQuery, Result<Models.IdentityUserDetails>>, GetIdentityUserQueryHandler>();
        services.AddScoped<ICommandHandler<GenerateEmailConfirmationTokenCommand, Result<Models.IdentityGeneratedToken>>, GenerateEmailConfirmationTokenCommandHandler>();
        services.AddScoped<ICommandHandler<ConfirmEmailCommand, Result>, ConfirmEmailCommandHandler>();
        services.AddScoped<ICommandHandler<ChangePasswordCommand, Result>, ChangePasswordCommandHandler>();
        services.AddScoped<ICommandHandler<GeneratePasswordResetTokenCommand, Result<Models.IdentityGeneratedToken>>, GeneratePasswordResetTokenCommandHandler>();
        services.AddScoped<ICommandHandler<ResetPasswordCommand, Result>, ResetPasswordCommandHandler>();
        services.AddScoped<ICommandHandler<CreateRoleCommand, Result>, CreateRoleCommandHandler>();
        services.AddScoped<ICommandHandler<AssignRoleCommand, Result>, AssignRoleCommandHandler>();
        services.AddScoped<ICommandHandler<AssignClaimCommand, Result>, AssignClaimCommandHandler>();

        services.AddScoped<IRequestValidator<RegisterIdentityUserCommand>, RegisterIdentityUserCommandValidator>();
        services.AddScoped<IRequestValidator<GetIdentityUserQuery>, GetIdentityUserQueryValidator>();
        services.AddScoped<IRequestValidator<GenerateEmailConfirmationTokenCommand>, GenerateEmailConfirmationTokenCommandValidator>();
        services.AddScoped<IRequestValidator<ConfirmEmailCommand>, ConfirmEmailCommandValidator>();
        services.AddScoped<IRequestValidator<ChangePasswordCommand>, ChangePasswordCommandValidator>();
        services.AddScoped<IRequestValidator<GeneratePasswordResetTokenCommand>, GeneratePasswordResetTokenCommandValidator>();
        services.AddScoped<IRequestValidator<ResetPasswordCommand>, ResetPasswordCommandValidator>();
        services.AddScoped<IRequestValidator<CreateRoleCommand>, CreateRoleCommandValidator>();
        services.AddScoped<IRequestValidator<AssignRoleCommand>, AssignRoleCommandValidator>();
        services.AddScoped<IRequestValidator<AssignClaimCommand>, AssignClaimCommandValidator>();

        return services;
    }
}
