using Vault.BuildingBlocks.Domain.Primitives;

namespace Vault.BuildingBlocks.Application.Abstractions;

public interface IRequestValidator<in TRequest>
{
    Task<IReadOnlyCollection<Error>> ValidateAsync(
        TRequest request,
        CancellationToken cancellationToken = default);
}
