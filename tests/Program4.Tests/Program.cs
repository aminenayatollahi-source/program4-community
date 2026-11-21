using Program4.Backtesting;

var options = new BacktestOptions(1_000m, 240m, 100m, 0m);
MarketTick[] ticks = [Tick(0, 100), Tick(1, 120), Tick(2, 150)];
BacktestResult result = Backtest.Run(ticks, new FirstSignal(), options);
Equal(1_054.60m, result.EndingCash);
Equal(5.40m, result.TotalFees);
Equal(0.24m, result.MaxDrawdownPct);
Equal(2, result.FillCount);
Equal(result, Backtest.Run(ticks, new FirstSignal(), options));

Console.WriteLine(FormattableString.Invariant(
    $"Next-tick example: fills={result.FillCount}, total fees={result.TotalFees:F2}, ending cash={result.EndingCash:F2}"));

MarketTick[] flat = [Tick(0, 100), Tick(1, 100), Tick(2, 100)];
BacktestResult slipped = Backtest.Run(flat, new FirstSignal(), new BacktestOptions(1_000m, 110m, 100m, 1_000m));
Equal(978m, slipped.EndingCash);
Equal(2m, slipped.TotalFees);
Equal(0, Backtest.Run(ticks[..2], new FirstSignal(), options).FillCount);
Equal(0, Backtest.Run(ticks, new FirstSignal(), options with { TradeNotional = 1_000m }).FillCount);

Throws<ArgumentException>(() => Backtest.Run([ticks[1], ticks[0]], new FirstSignal(), options));
Throws<ArgumentException>(() => Backtest.Run([ticks[0], ticks[1] with { Symbol = "OTHER" }], new FirstSignal(), options));
Throws<ArgumentOutOfRangeException>(() => Backtest.Run(ticks, new FirstSignal(), options with { SlippageBps = 10_000 }));
Throws<ArgumentException>(() => new MovingAverageCross(3, 2));
var csv = new StringReader(TickCsv.Header + "\n2026-01-01T00:00:00.0000000+00:00,DEMO,99.9,100.1,100\n");
Equal(99.9m, TickCsv.Read(csv, "DEMO").Single().Bid);
Throws<FormatException>(() => TickCsv.Read(new StringReader(TickCsv.Header + "\nbad,DEMO,1,1,1"), "DEMO"));
Throws<FormatException>(() => TickCsv.Read(new StringReader(TickCsv.Header), "DEMO"));

const string ZonedTick = "\n2026-01-01T03:30:02.123+03:30,DEMO,99.9,100.1,100";
const string UtcTick = "\n2026-01-01T00:00:02.123Z,DEMO,99.9,100.1,100";
const string PlainUtcTick = "\n2026-01-01T00:00:03Z,DEMO,99.9,100.1,100";
const string PlainOffsetTick = "\n2026-01-01T04:00:03+04:00,DEMO,99.9,100.1,100";
Equal(ReadTick(UtcTick).Timestamp, ReadTick(ZonedTick).Timestamp);
Equal(ReadTick(PlainUtcTick).Timestamp, ReadTick(PlainOffsetTick).Timestamp);
Equal(TimeSpan.FromHours(3.5), ReadTick(ZonedTick).Timestamp.Offset);
Equal(TimeSpan.Zero, ReadTick(UtcTick).Timestamp.Offset);
Throws<FormatException>(() => ReadTick("\n2026-01-01T00:00:02,DEMO,99.9,100.1,100"), "Line 2: timestamp");
Throws<FormatException>(() => ReadTick("\n\n2026-01-01T00:00:02Z,DEMO,0,100.1,100"), "Line 3: bid");
Throws<FormatException>(() => ReadTick("\n2026-01-01T00:00:02Z,DEMO,99.9,99.8,100"), "Line 2: ask");

Console.WriteLine("PASS: next-tick execution, costs, drawdown, repeatability, and input validation.");
return 0;

static MarketTick ReadTick(string row) => TickCsv.Read(new StringReader(TickCsv.Header + row), "DEMO").Single();

static MarketTick Tick(int minute, decimal price) =>
    new(new DateTimeOffset(2026, 1, 1, 0, minute, 0, TimeSpan.Zero), "DEMO", price, price, price);

static void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new InvalidOperationException($"Expected {expected}, got {actual}.");
}

static void Throws<T>(Action action, string? messagePrefix = null) where T : Exception
{
    try { action(); }
    catch (T error)
    {
        if (messagePrefix is not null && !error.Message.StartsWith(messagePrefix, StringComparison.Ordinal))
            throw new InvalidOperationException($"Expected an error starting with {messagePrefix}, got {error.Message}.");
        return;
    }
    throw new InvalidOperationException($"Expected {typeof(T).Name}.");
}

sealed class FirstSignal : IHistoricalSignalStrategy
{
    private bool sent;

    public Signal OnTick(MarketTick tick)
    {
        if (sent)
            return Signal.Hold;
        sent = true;
        return Signal.Enter;
    }
}
