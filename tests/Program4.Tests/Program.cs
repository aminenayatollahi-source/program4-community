using Program4.Backtesting;

var options = new BacktestOptions(1_000m, 240m, 100m, 0m);
MarketTick[] ticks = [Tick(0, 100), Tick(1, 120), Tick(2, 150)];
BacktestResult result = Backtest.Run(ticks, new FirstSignal(), options);
Equal(1_054.60m, result.EndingCash);
Equal(5.40m, result.TotalFees);
Equal(0.24m, result.MaxDrawdownPct);
Equal(2, result.FillCount);
Equal(result, Backtest.Run(ticks, new FirstSignal(), options));

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

Console.WriteLine("PASS: next-tick execution, costs, drawdown, repeatability, and input validation.");
return 0;

static MarketTick Tick(int minute, decimal price) =>
    new(new DateTimeOffset(2026, 1, 1, 0, minute, 0, TimeSpan.Zero), "DEMO", price, price, price);

static void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new InvalidOperationException($"Expected {expected}, got {actual}.");
}

static void Throws<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { return; }
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
