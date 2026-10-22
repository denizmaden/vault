using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Ledger.Domain.Errors;

namespace Vault.Services.Ledger.Domain.ValueObjects;

public sealed class MoneyAmount : ValueObject
{
    private MoneyAmount(decimal value)
    {
        Value = decimal.Round(value, 2, MidpointRounding.ToZero);
    }

    private MoneyAmount()
    {
    }

    public decimal Value { get; private set; }

    public static Result<MoneyAmount> Create(decimal value)
    {
        if (value <= 0)
        {
            return Result<MoneyAmount>.Failure(LedgerErrors.InvalidAmount);
        }

        return Result<MoneyAmount>.Success(new MoneyAmount(value));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString("0.00");
}

