using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Reporting.Domain.Errors;

namespace Vault.Services.Reporting.Domain.ValueObjects;

public sealed class ReportingCurrency : ValueObject
{
    private ReportingCurrency(string code)
    {
        Code = code;
    }

    public string Code { get; }

    public static Result<ReportingCurrency> Create(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result<ReportingCurrency>.Failure(ReportingErrors.InvalidCurrency);
        }

        var normalized = code.Trim().ToUpperInvariant();
        if (normalized.Length is < 3 or > 5)
        {
            return Result<ReportingCurrency>.Failure(ReportingErrors.InvalidCurrency);
        }

        return Result<ReportingCurrency>.Success(new ReportingCurrency(normalized));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Code;
    }
}
