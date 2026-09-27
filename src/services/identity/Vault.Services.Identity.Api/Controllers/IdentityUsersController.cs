using Duende.IdentityServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Api.Contracts;
using Vault.Services.Identity.Application.Features.AssignClaim;
using Vault.Services.Identity.Application.Features.AssignRole;
using Vault.Services.Identity.Application.Features.ChangePassword;
using Vault.Services.Identity.Application.Features.ConfirmEmail;
using Vault.Services.Identity.Application.Features.GenerateEmailConfirmationToken;
using Vault.Services.Identity.Application.Features.GeneratePasswordResetToken;
using Vault.Services.Identity.Application.Features.GetIdentityUser;
using Vault.Services.Identity.Application.Features.RegisterIdentityUser;
using Vault.Services.Identity.Application.Features.ResetPassword;
using Vault.Services.Identity.Application.Models;

namespace Vault.Services.Identity.Api.Controllers;

[
    ApiController,
    Route("api/identity-users")
]
public sealed class IdentityUsersController(
    ICommandDispatcher commandDispatcher,
    IQueryDispatcher queryDispatcher) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<RegisterIdentityUserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterIdentityUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.DispatchAsync<RegisterIdentityUserCommand, Result<RegisterIdentityUserResponse>>(
            new RegisterIdentityUserCommand(request.Email, request.Password),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Register), result.Value)
            : ToFailureResult(result);
    }

    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    [HttpGet("{userId:guid}")]
    [ProducesResponseType<IdentityUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid userId, CancellationToken cancellationToken)
    {
        var result = await queryDispatcher.DispatchAsync<GetIdentityUserQuery, Result<IdentityUserDetails>>(
            new GetIdentityUserQuery(userId),
            cancellationToken);

        return result.IsSuccess
            ? Ok(ToResponse(result.Value))
            : ToFailureResult(result);
    }

    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    [HttpPost("{userId:guid}/email-confirmation-token")]
    [ProducesResponseType<IdentityGeneratedTokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateEmailConfirmationToken(Guid userId, CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.DispatchAsync<GenerateEmailConfirmationTokenCommand, Result<IdentityGeneratedToken>>(
            new GenerateEmailConfirmationTokenCommand(userId),
            cancellationToken);

        return result.IsSuccess
            ? Ok(ToResponse(result.Value))
            : ToFailureResult(result);
    }

    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    [HttpPost("{userId:guid}/confirm-email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmEmail(
        Guid userId,
        [FromBody] ConfirmEmailRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.DispatchAsync<ConfirmEmailCommand, Result>(
            new ConfirmEmailCommand(userId, request.Token),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : ToFailureResult(result);
    }

    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    [HttpPost("{userId:guid}/change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(
        Guid userId,
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.DispatchAsync<ChangePasswordCommand, Result>(
            new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : ToFailureResult(result);
    }

    [HttpPost("password-reset-token")]
    [ProducesResponseType<IdentityGeneratedTokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GeneratePasswordResetToken(
        [FromBody] GeneratePasswordResetTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.DispatchAsync<GeneratePasswordResetTokenCommand, Result<IdentityGeneratedToken>>(
            new GeneratePasswordResetTokenCommand(request.Email),
            cancellationToken);

        return result.IsSuccess
            ? Ok(ToResponse(result.Value))
            : ToFailureResult(result);
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.DispatchAsync<ResetPasswordCommand, Result>(
            new ResetPasswordCommand(request.UserId, request.Token, request.NewPassword),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : ToFailureResult(result);
    }

    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    [HttpPost("{userId:guid}/roles")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignRole(
        Guid userId,
        [FromBody] AssignRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.DispatchAsync<AssignRoleCommand, Result>(
            new AssignRoleCommand(userId, request.RoleName),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : ToFailureResult(result);
    }

    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    [HttpPost("{userId:guid}/claims")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignClaim(
        Guid userId,
        [FromBody] AssignClaimRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.DispatchAsync<AssignClaimCommand, Result>(
            new AssignClaimCommand(userId, request.ClaimType, request.ClaimValue),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : ToFailureResult(result);
    }

    private IActionResult ToFailureResult(Result<RegisterIdentityUserResponse> result)
        => ToFailureResult((Result)result);

    private IActionResult ToFailureResult(Result<IdentityUserDetails> result)
        => ToFailureResult((Result)result);

    private IActionResult ToFailureResult(Result<IdentityGeneratedToken> result)
        => ToFailureResult((Result)result);

    private IActionResult ToFailureResult(Result result)
    {
        var statusCode = result.Error.Code switch
        {
            "identity.duplicate_email" => StatusCodes.Status409Conflict,
            "identity.duplicate_role" => StatusCodes.Status409Conflict,
            "identity.user_not_found" => StatusCodes.Status404NotFound,
            "identity.role_not_found" => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status400BadRequest
        };

        return Problem("Identity operation failed.", result.Error.Description, statusCode: statusCode);
    }

    private static IdentityUserResponse ToResponse(IdentityUserDetails details) =>
        new(
            details.UserId,
            details.Email,
            details.EmailConfirmed,
            details.CreatedOnUtc,
            details.Roles,
            details.Claims.Select(claim => new IdentityUserClaimResponse(claim.Type, claim.Value)).ToArray());

    private static IdentityGeneratedTokenResponse ToResponse(IdentityGeneratedToken token) =>
        new(token.UserId, token.Purpose, token.Token);
}
