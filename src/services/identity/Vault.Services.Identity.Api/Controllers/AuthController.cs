using Duende.IdentityServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Vault.Services.Identity.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var claims = User.Claims
            .Select(claim => new
            {
                claim.Type,
                claim.Value
            });

        return Ok(new
        {
            Subject = User.FindFirst("sub")?.Value,
            Name = User.Identity?.Name,
            Claims = claims
        });
    }
}
