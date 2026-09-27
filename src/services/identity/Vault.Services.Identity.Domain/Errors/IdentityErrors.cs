using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Identity.Domain.Errors;

public static class IdentityErrors
{
    public static readonly Error InvalidEmail = new(
        "identity.invalid_email",
        "The provided email address is invalid.");

    public static readonly Error InvalidPassword = new(
        "identity.invalid_password",
        "The provided password does not meet the minimum requirements.");

    public static readonly Error DuplicateEmail = new(
        "identity.duplicate_email",
        "An identity user with the same email already exists.");

    public static readonly Error UserNotFound = new(
        "identity.user_not_found",
        "The requested identity user was not found.");

    public static readonly Error RoleNotFound = new(
        "identity.role_not_found",
        "The requested identity role was not found.");

    public static readonly Error DuplicateRole = new(
        "identity.duplicate_role",
        "An identity role with the same name already exists.");

    public static readonly Error InvalidToken = new(
        "identity.invalid_token",
        "The provided token is invalid.");

    public static readonly Error InvalidClaim = new(
        "identity.invalid_claim",
        "The provided claim type or value is invalid.");

    public static readonly Error InvalidRoleName = new(
        "identity.invalid_role_name",
        "The provided role name is invalid.");

    public static readonly Error EmailAlreadyConfirmed = new(
        "identity.email_already_confirmed",
        "The email address has already been confirmed.");

    public static readonly Error RoleAlreadyAssigned = new(
        "identity.role_already_assigned",
        "The identity account already has the requested role.");

    public static readonly Error ClaimAlreadyAssigned = new(
        "identity.claim_already_assigned",
        "The identity account already has the requested claim.");
}
