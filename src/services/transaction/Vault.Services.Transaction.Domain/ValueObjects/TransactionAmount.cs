using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Transaction.Domain.Errors;

namespace Vault.Services.Transaction.Domain.ValueObjects;

public sealed class TransactionAmount : ValueObject
{
    private TransactionAmount(decimal value)
    {
        Value = decimal.Round(value, 2, MidpointRounding.ToZero);
    }

    private TransactionAmount()
    {
    }

    public decimal Value { get; private set; }

    public static Result<TransactionAmount> Create(decimal value)
    {
        if (value <= 0)
        {
            return Result<TransactionAmount>.Failure(TransactionErrors.InvalidAmount);
        }

        return Result<TransactionAmount>.Success(new TransactionAmount(value));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
