using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Reporting.Domain.Errors;

namespace Vault.Services.Reporting.Domain.ValueObjects;

public sealed class ReportingAmount : ValueObject
{
    private ReportingAmount(decimal value)
    {
        Value = decimal.Round(value, 2, MidpointRounding.ToEven);
    }

    public decimal Value { get; }

    public static Result<ReportingAmount> Create(decimal value)
    {
        if (value <= 0)
        {
            return Result<ReportingAmount>.Failure(ReportingErrors.InvalidAmount);
        }

        return Result<ReportingAmount>.Success(new ReportingAmount(value));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
