using System;
using System.Collections.Generic;
using System.Text;

namespace Vault.BuildingBlocks.Api.Correlation
{
    public sealed class CorrelationContextAccessor
    {
        public string? CorrelationId { get; internal set; }
    }
}
