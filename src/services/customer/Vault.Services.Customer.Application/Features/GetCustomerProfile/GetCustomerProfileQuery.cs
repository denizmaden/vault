using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Customer.Application.Models;

namespace Vault.Services.Customer.Application.Features.GetCustomerProfile;

public sealed record GetCustomerProfileQuery(Guid CustomerId) : IQuery<Result<CustomerProfileDetails>>;
