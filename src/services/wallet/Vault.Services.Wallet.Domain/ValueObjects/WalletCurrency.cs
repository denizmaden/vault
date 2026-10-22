using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Wallet.Domain.Errors;

namespace Vault.Services.Wallet.Domain.ValueObjects;

public sealed class WalletCurrency : ValueObject
{
    private WalletCurrency(string code)
    {
        Code = code;
    }

    private WalletCurrency()
    {
        Code = null!;
    }

    public string Code { get; private set; }

    public static Result<WalletCurrency> Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result<WalletCurrency>.Failure(WalletErrors.InvalidCurrency);
        }

        var normalized = code.Trim().ToUpperInvariant();

        if (normalized.Length != 3 || normalized.Any(ch => !char.IsAsciiLetter(ch)))
        {
            return Result<WalletCurrency>.Failure(WalletErrors.InvalidCurrency);
        }

        return Result<WalletCurrency>.Success(new WalletCurrency(normalized));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Code;
    }
}

