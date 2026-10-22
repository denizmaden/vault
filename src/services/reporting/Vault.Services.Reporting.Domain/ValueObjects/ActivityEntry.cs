using Vault.BuildingBlocks.Domain.Primitives;
using Vault.Services.Reporting.Domain.Enums;
using Vault.Services.Reporting.Domain.Errors;

namespace Vault.Services.Reporting.Domain.ValueObjects;

public sealed class ActivityEntry : ValueObject
{
    private ActivityEntry(
        ReportEntryType type,
        decimal amount,
        string reference,
        int version,
        DateTime occurredOnUtc)
    {
        Type = type;
        Amount = amount;
        Reference = reference;
        Version = version;
        OccurredOnUtc = occurredOnUtc;
    }

    public ReportEntryType Type { get; }

    public decimal Amount { get; }

    public string Reference { get; }

    public int Version { get; }

    public DateTime OccurredOnUtc { get; }

    public static Result<ActivityEntry> Create(
        ReportEntryType type,
        decimal amount,
        string? reference,
        int version,
        DateTime occurredOnUtc)
    {
        if (version < 1)
        {
            return Result<ActivityEntry>.Failure(ReportingErrors.InvalidVersion);
        }

        if (type != ReportEntryType.Opened && amount <= 0)
        {
            return Result<ActivityEntry>.Failure(ReportingErrors.InvalidAmount);
        }

        var normalizedReference = string.IsNullOrWhiteSpace(reference) ? "system" : reference.Trim();
        if (normalizedReference.Length > 150)
        {
            return Result<ActivityEntry>.Failure(ReportingErrors.InvalidReference);
        }

        return Result<ActivityEntry>.Success(new ActivityEntry(type, amount, normalizedReference, version, occurredOnUtc));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type;
        yield return Amount;
        yield return Reference;
        yield return Version;
        yield return OccurredOnUtc;
    }
}
