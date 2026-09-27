using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Domain.Errors;
using Vault.Services.Identity.Domain.ValueObjects;

namespace Vault.Services.Identity.Domain.Entities;

public sealed class IdentityAccount : AggregateRoot<Guid>
{
    private readonly HashSet<RoleName> _roles;
    private readonly HashSet<IdentityAccountClaim> _claims;

    private IdentityAccount(
        Guid id,
        Email email,
        bool emailConfirmed,
        DateTime createdOnUtc,
        IEnumerable<RoleName>? roles = null,
        IEnumerable<IdentityAccountClaim>? claims = null) : base(id)
    {
        Email = email;
        EmailConfirmed = emailConfirmed;
        CreatedOnUtc = createdOnUtc;
        _roles = roles is null ? [] : [.. roles];
        _claims = claims is null ? [] : [.. claims];
    }

    public Email Email { get; }

    public bool EmailConfirmed { get; private set; }

    public DateTime CreatedOnUtc { get; }

    public IReadOnlyCollection<RoleName> Roles => _roles.ToArray();

    public IReadOnlyCollection<IdentityAccountClaim> Claims => _claims.ToArray();

    public static IdentityAccount Register(Guid id, Email email, DateTime createdOnUtc) =>
        new(id, email, emailConfirmed: false, createdOnUtc);

    public static IdentityAccount Rehydrate(
        Guid id,
        Email email,
        bool emailConfirmed,
        DateTime createdOnUtc,
        IEnumerable<RoleName> roles,
        IEnumerable<IdentityAccountClaim> claims) =>
        new(id, email, emailConfirmed, createdOnUtc, roles, claims);

    public Result ConfirmEmail()
    {
        if (EmailConfirmed)
        {
            return Result.Failure(IdentityErrors.EmailAlreadyConfirmed);
        }

        EmailConfirmed = true;
        return Result.Success();
    }

    public Result AssignRole(RoleName roleName)
    {
        if (_roles.Contains(roleName))
        {
            return Result.Failure(IdentityErrors.RoleAlreadyAssigned);
        }

        _roles.Add(roleName);
        return Result.Success();
    }

    public Result AssignClaim(IdentityAccountClaim claim)
    {
        if (_claims.Contains(claim))
        {
            return Result.Failure(IdentityErrors.ClaimAlreadyAssigned);
        }

        _claims.Add(claim);
        return Result.Success();
    }
}
