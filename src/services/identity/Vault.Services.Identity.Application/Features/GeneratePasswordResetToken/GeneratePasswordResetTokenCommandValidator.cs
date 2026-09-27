using Vault.BuildingBlocks.Application.Abstractions;
using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Identity.Domain.ValueObjects;

namespace Vault.Services.Identity.Application.Features.GeneratePasswordResetToken;

public sealed class GeneratePasswordResetTokenCommandValidator : IRequestValidator<GeneratePasswordResetTokenCommand>
{
    public Task<IReadOnlyCollection<Error>> ValidateAsync(
        GeneratePasswordResetTokenCommand request,
        CancellationToken cancellationToken = default)
    {
        var emailResult = Email.Create(request.Email);
        IReadOnlyCollection<Error> errors = emailResult.IsFailure
            ? [emailResult.Error]
            : [];

        return Task.FromResult(errors);
    }
}
