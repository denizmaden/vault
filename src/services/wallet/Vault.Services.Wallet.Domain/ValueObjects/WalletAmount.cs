using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Wallet.Domain.Errors;

namespace Vault.Services.Wallet.Domain.ValueObjects;

public sealed class WalletAmount : ValueObject
{
    private WalletAmount(decimal value)
    {
        Value = decimal.Round(value, 2, MidpointRounding.ToZero);
    }

    private WalletAmount()
    {
    }

    public decimal Value { get; private set; }

    public static Result<WalletAmount> Create(decimal value)
    {
        if (value <= 0)
        {
            return Result<WalletAmount>.Failure(WalletErrors.InvalidAmount);
        }

        return Result<WalletAmount>.Success(new WalletAmount(value));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}

