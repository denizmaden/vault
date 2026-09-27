using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Vault.BuildingBlocks.Api.Authentication;
using Vault.BuildingBlocks.Api.Correlation;

namespace Vault.BuildingBlocks.Api
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBuildingBlocksApi(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<CorrelationContextAccessor>();

            return services;
        }

        public static IServiceCollection AddVaultApiAuthentication(
            this IServiceCollection services,
            IConfiguration configuration,
            string apiScope,
            string apiAudience)
        {
            var settings = configuration
                .GetSection(VaultApiAuthenticationDefaults.AuthenticationSectionName)
                .Get<VaultApiAuthenticationSettings>()
                ?? throw new InvalidOperationException("Authentication configuration is missing.");

            if (string.IsNullOrWhiteSpace(settings.Authority))
            {
                throw new InvalidOperationException("Authentication authority is missing.");
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = settings.Authority;

                    if (!string.IsNullOrWhiteSpace(settings.MetadataAddress))
                    {
                        options.MetadataAddress = settings.MetadataAddress;
                    }

                    options.RequireHttpsMetadata = settings.RequireHttpsMetadata;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidAudience = apiAudience,
                        ValidateIssuer = true
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    VaultApiAuthenticationDefaults.ScopePolicyName,
                    policy =>
                    {
                        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                        policy.RequireClaim("scope", apiScope);
                    });
            });

            return services;
        }
    }
}
