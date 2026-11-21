using System.Globalization;

namespace Program4.Backtesting;

public static class TickCsv
{
    public const string Header = "timestamp,symbol,bid,ask,price";

    private static readonly string[] TimestampFormats =
    [
        "yyyy-MM-dd'T'HH:mm:ss'Z'",
        "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'",
        "yyyy-MM-dd'T'HH:mm:sszzz",
        "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFzzz"
    ];

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
            var tick = ParseTick(fields, lineNumber);
            if (StringComparer.Ordinal.Equals(tick.Symbol, symbol))
                ticks.Add(tick);
        }

        return ticks.Count > 0 ? ticks : throw new FormatException("The CSV contains no rows for the requested symbol.");
    }

    private static MarketTick ParseTick(string[] fields, int lineNumber)
    {
        if (fields.Length != 5)
            throw new FormatException($"Line {lineNumber}: expected five unquoted columns.");
        if (string.IsNullOrWhiteSpace(fields[1]))
            throw new FormatException($"Line {lineNumber}: symbol is required.");
        if (!DateTimeOffset.TryParseExact(fields[0], TimestampFormats, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out DateTimeOffset timestamp))
            throw new FormatException($"Line {lineNumber}: timestamp must be ISO 8601 with an explicit offset, such as 2026-01-01T00:00:00Z.");

        decimal bid = ReadPrice(fields[2], "bid", lineNumber);
        decimal ask = ReadPrice(fields[3], "ask", lineNumber);
        decimal price = ReadPrice(fields[4], "price", lineNumber);
        if (ask < bid)
            throw new FormatException($"Line {lineNumber}: ask must be greater than or equal to bid.");

        return new MarketTick(timestamp, fields[1], bid, ask, price);
    }

    private static decimal ReadPrice(string field, string name, int lineNumber)
    {
        if (!decimal.TryParse(field, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal price) || price <= 0)
            throw new FormatException($"Line {lineNumber}: {name} must be a positive decimal using a period as the decimal separator.");

        return price;
    }
}
