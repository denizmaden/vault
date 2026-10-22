using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Transaction.Domain.Errors;

namespace Vault.Services.Transaction.Domain.ValueObjects;

public sealed class TransactionCurrency : ValueObject
{
    private TransactionCurrency(string code)
    {
        Code = code;
    }

    private TransactionCurrency()
    {
        Code = null!;
    }

    public string Code { get; private set; }

    public static Result<TransactionCurrency> Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result<TransactionCurrency>.Failure(TransactionErrors.InvalidCurrency);
        }

        var normalized = code.Trim().ToUpperInvariant();
        if (normalized.Length != 3 || normalized.Any(static character => !char.IsAsciiLetter(character)))
        {
            return Result<TransactionCurrency>.Failure(TransactionErrors.InvalidCurrency);
        }

        return Result<TransactionCurrency>.Success(new TransactionCurrency(normalized));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Code;
    }
}
