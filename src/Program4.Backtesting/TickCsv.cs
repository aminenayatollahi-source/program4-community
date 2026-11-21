using System.Globalization;

namespace Program4.Backtesting;

public static class TickCsv
{
    public const string Header = "timestamp,symbol,bid,ask,price";

    public static IReadOnlyList<MarketTick> Read(TextReader input, string symbol)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        if (!StringComparer.Ordinal.Equals(input.ReadLine(), Header))
            throw new FormatException($"Expected CSV header: {Header}");

        List<MarketTick> ticks = new();
        string? line;
        int lineNumber = 1;
        while ((line = input.ReadLine()) is not null)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
                continue;
            string[] fields = line.Split(',', StringSplitOptions.TrimEntries);
            if (fields.Length != 5 || string.IsNullOrWhiteSpace(fields[1]) ||
                !DateTimeOffset.TryParseExact(fields[0], "O", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTimeOffset timestamp) ||
                !TryPrice(fields[2], out decimal bid) || !TryPrice(fields[3], out decimal ask) ||
                !TryPrice(fields[4], out decimal price) || bid <= 0 || ask < bid || price <= 0)
                throw new FormatException($"Invalid tick on line {lineNumber}.");

            if (StringComparer.Ordinal.Equals(fields[1], symbol))
                ticks.Add(new MarketTick(timestamp, symbol, bid, ask, price));
        }

        return ticks.Count > 0 ? ticks : throw new FormatException("The CSV contains no rows for the requested symbol.");
    }

    private static bool TryPrice(string field, out decimal price) =>
        decimal.TryParse(field, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out price);
}
