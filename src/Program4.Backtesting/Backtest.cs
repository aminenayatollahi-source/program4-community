namespace Program4.Backtesting;

public sealed record MarketTick(DateTimeOffset Timestamp, string Symbol, decimal Bid, decimal Ask, decimal Price);
public enum Signal { Hold, Enter, Exit }
public interface IHistoricalSignalStrategy
{
    Signal OnTick(MarketTick tick);
}

public sealed record BacktestOptions(
    decimal StartingCash = 100_000m,
    decimal TradeNotional = 2_000m,
    decimal FeeBps = 5m,
    decimal SlippageBps = 2m);

public sealed record BacktestResult(
    string Symbol,
    int TickCount,
    int FillCount,
    decimal StartingCash,
    decimal EndingCash,
    decimal NetReturnPct,
    decimal MaxDrawdownPct,
    decimal TotalFees);

public static class Backtest
{
    public static BacktestResult Run(
        IReadOnlyList<MarketTick> ticks, IHistoricalSignalStrategy strategy, BacktestOptions options)
    {
        ArgumentNullException.ThrowIfNull(ticks);
        ArgumentNullException.ThrowIfNull(strategy);
        ArgumentNullException.ThrowIfNull(options);
        if (ticks.Count == 0)
            throw new ArgumentException("At least one tick is required.", nameof(ticks));
        if (options.StartingCash <= 0 || options.TradeNotional <= 0 || options.FeeBps < 0 ||
            options.SlippageBps is < 0 or >= 10_000)
            throw new ArgumentOutOfRangeException(nameof(options), "Capital and trade size must be positive, fees nonnegative, and slippage below 10,000 bps.");

        string symbol = ticks[0].Symbol;
        for (int i = 0; i < ticks.Count; i++)
        {
            MarketTick tick = ticks[i];
            if (string.IsNullOrWhiteSpace(symbol) || !StringComparer.Ordinal.Equals(tick.Symbol, symbol) ||
                tick.Bid <= 0 || tick.Ask < tick.Bid || tick.Price <= 0 ||
                (i > 0 && tick.Timestamp <= ticks[i - 1].Timestamp))
                throw new ArgumentException("Ticks must have one symbol, positive prices, a valid spread, and strictly increasing timestamps.", nameof(ticks));
        }

        decimal feeRate = options.FeeBps / 10_000m;
        decimal slippage = options.SlippageBps / 10_000m;
        decimal cash = options.StartingCash;
        decimal quantity = 0;
        decimal fees = 0;
        decimal peak = cash;
        decimal maxDrawdown = 0;
        int fills = 0;
        Signal pending = Signal.Hold;

        for (int i = 0; i < ticks.Count; i++)
        {
            MarketTick tick = ticks[i];
            bool last = i == ticks.Count - 1;
            if (pending == Signal.Enter && quantity == 0 && !last)
            {
                decimal fee = options.TradeNotional * feeRate;
                if (cash >= options.TradeNotional + fee)
                {
                    quantity = options.TradeNotional / (tick.Ask * (1 + slippage));
                    cash -= options.TradeNotional + fee;
                    fees += fee;
                    fills++;
                }
            }
            else if (quantity > 0 && (pending == Signal.Exit || last))
            {
                decimal proceeds = quantity * tick.Bid * (1 - slippage);
                decimal fee = proceeds * feeRate;
                cash += proceeds - fee;
                quantity = 0;
                fees += fee;
                fills++;
            }

            decimal equity = cash + quantity * tick.Bid;
            peak = Math.Max(peak, equity);
            maxDrawdown = Math.Max(maxDrawdown, (peak - equity) / peak * 100m);
            if (!last)
                pending = strategy.OnTick(tick);
        }

        return new BacktestResult(symbol, ticks.Count, fills, options.StartingCash, cash,
            (cash - options.StartingCash) / options.StartingCash * 100m, maxDrawdown, fees);
    }
}
