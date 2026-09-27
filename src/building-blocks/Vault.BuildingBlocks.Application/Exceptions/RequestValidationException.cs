using System;
using System.Collections.Generic;
using System.Text;
using Vault.BuildingBlocks.Domain.Primitives;


namespace Vault.BuildingBlocks.Application.Exceptions
{
    public sealed class RequestValidationException(IReadOnlyCollection<Error> errors)
    : Exception("One or more request validation errors occurred.")
    {
        public IReadOnlyCollection<Error> Errors { get; } = errors;
    }
}
