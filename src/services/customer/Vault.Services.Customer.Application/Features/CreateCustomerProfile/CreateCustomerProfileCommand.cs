using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Customer.Application.Models;

namespace Vault.Services.Customer.Application.Features.CreateCustomerProfile;

public sealed record CreateCustomerProfileCommand(
    Guid IdentityUserId,
    string FirstName,
    string LastName,
    string DisplayName,
    string PreferredLanguage) : ICommand<Result<CustomerProfileCreated>>;
