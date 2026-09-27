using Duende.IdentityServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Api.Contracts;
using Vault.Services.Identity.Application.Features.CreateRole;

namespace Vault.Services.Identity.Api.Controllers;

[ApiController]
[Authorize(IdentityServerConstants.LocalApi.PolicyName)]
[Route("api/identity-roles")]
public sealed class IdentityRolesController(ICommandDispatcher commandDispatcher) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.DispatchAsync<CreateRoleCommand, Result>(
            new CreateRoleCommand(request.RoleName),
            cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created)
            : ToFailureResult(result);
    }

    private IActionResult ToFailureResult(Result result)
    {
        var statusCode = result.Error.Code switch
        {
            "identity.duplicate_role" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return Problem(
            title: "Identity role operation failed.",
            detail: result.Error.Description,
            statusCode: statusCode);
    }
}
