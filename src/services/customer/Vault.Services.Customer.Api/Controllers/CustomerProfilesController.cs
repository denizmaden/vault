using Microsoft.AspNetCore.Mvc;
using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Customer.Api.Contracts;
using Vault.Services.Customer.Application.Features.CompleteCustomerOnboarding;
using Vault.Services.Customer.Application.Features.CreateCustomerProfile;
using Vault.Services.Customer.Application.Features.GetCustomerProfile;
using Vault.Services.Customer.Application.Features.UpdateCustomerProfile;
using Vault.Services.Customer.Application.Models;

namespace Vault.Services.Customer.Api.Controllers;

[ApiController]
[Route("api/customer-profiles")]
public sealed class CustomerProfilesController(
    ICommandDispatcher commandDispatcher,
    IQueryDispatcher queryDispatcher) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CreateCustomerProfileResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomerProfileRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.DispatchAsync<CreateCustomerProfileCommand, Result<CustomerProfileCreated>>(
            new CreateCustomerProfileCommand(
                request.IdentityUserId,
                request.FirstName,
                request.LastName,
                request.DisplayName,
                request.PreferredLanguage),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { customerId = result.Value.CustomerId }, ToResponse(result.Value))
            : ToFailureResult(result);
    }

    [HttpGet("{customerId:guid}")]
    [ProducesResponseType<CustomerProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await queryDispatcher.DispatchAsync<GetCustomerProfileQuery, Result<CustomerProfileDetails>>(
            new GetCustomerProfileQuery(customerId),
            cancellationToken);

        return result.IsSuccess
            ? Ok(ToResponse(result.Value))
            : ToFailureResult(result);
    }

    [HttpPut("{customerId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid customerId,
        [FromBody] UpdateCustomerProfileRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.DispatchAsync<UpdateCustomerProfileCommand, Result>(
            new UpdateCustomerProfileCommand(
                customerId,
                request.FirstName,
                request.LastName,
                request.DisplayName,
                request.PreferredLanguage),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : ToFailureResult(result);
    }

    [HttpPost("{customerId:guid}/complete-onboarding")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteOnboarding(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.DispatchAsync<CompleteCustomerOnboardingCommand, Result>(
            new CompleteCustomerOnboardingCommand(customerId),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : ToFailureResult(result);
    }

    private IActionResult ToFailureResult(Result<CustomerProfileCreated> result)
        => ToFailureResult((Result)result);

    private IActionResult ToFailureResult(Result<CustomerProfileDetails> result)
        => ToFailureResult((Result)result);

    private IActionResult ToFailureResult(Result result)
    {
        var statusCode = result.Error.Code switch
        {
            "customer.not_found" => StatusCodes.Status404NotFound,
            "customer.duplicate_identity_user_id" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return Problem("Customer operation failed.", result.Error.Description, statusCode: statusCode);
    }

    private static CreateCustomerProfileResponse ToResponse(CustomerProfileCreated created) =>
        new(created.CustomerId, created.IdentityUserId, created.OnboardingStatus.ToString());

    private static CustomerProfileResponse ToResponse(CustomerProfileDetails details) =>
        new(
            details.CustomerId,
            details.IdentityUserId,
            details.FirstName,
            details.LastName,
            details.DisplayName,
            details.PreferredLanguage,
            details.OnboardingStatus.ToString(),
            details.CreatedOnUtc,
            details.OnboardingCompletedOnUtc);
}
