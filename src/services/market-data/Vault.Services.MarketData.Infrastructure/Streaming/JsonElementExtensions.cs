using System.Globalization;
using System.Text.Json;

namespace Vault.Services.MarketData.Infrastructure.Streaming;

internal static class JsonElementExtensions
{
    public static bool TryGetDecimalValue(this JsonElement element, out decimal value)
    {
        if (element.ValueKind == JsonValueKind.Number)
        {
            return element.TryGetDecimal(out value);
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            return decimal.TryParse(
                element.GetString(),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out value);
        }

        value = default;
        return false;
    }

    public static DateTime ReadUnixMillisecondsAsUtc(this JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out var milliseconds))
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime;
        }

        return DateTime.UtcNow;
    }
}
