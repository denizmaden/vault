using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.Services.Customer.Application.Features.UpdateCustomerProfile;

public sealed record UpdateCustomerProfileCommand(
    Guid CustomerId,
    string FirstName,
    string LastName,
    string DisplayName,
    string PreferredLanguage) : ICommand<Result>;
