using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Ledger.Domain.Errors;

namespace Vault.Services.Ledger.Domain.ValueObjects;

public sealed class Currency : ValueObject
{
    private Currency(string code)
    {
        Code = code;
    }

    private Currency()
    {
        Code = null!;
    }

    public string Code { get; private set; }

    public static Result<Currency> Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result<Currency>.Failure(LedgerErrors.InvalidCurrency);
        }

        var normalized = code.Trim().ToUpperInvariant();

        if (normalized.Length != 3 || normalized.Any(ch => !char.IsAsciiLetter(ch)))
        {
            return Result<Currency>.Failure(LedgerErrors.InvalidCurrency);
        }

        return Result<Currency>.Success(new Currency(normalized));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Code;
    }

    public override string ToString() => Code;
}

