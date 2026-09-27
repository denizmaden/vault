using Microsoft.AspNetCore.Builder;
using Vault.BuildingBlocks.Api.Middlewares;

namespace Vault.BuildingBlocks.Api
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseBuildingBlocksApi(this IApplicationBuilder app)
        {
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            return app;
        }
    }
}
