namespace Program4.Backtesting;

public sealed class MovingAverageCross : IHistoricalSignalStrategy
{
    private readonly int fast;
    private readonly int slow;
    private readonly Queue<decimal> fastPrices = new();
    private readonly Queue<decimal> slowPrices = new();
    private decimal fastSum;
    private decimal slowSum;
    private bool? wasAbove;

    public MovingAverageCross(int fast, int slow)
    {
        if (fast < 1 || slow <= fast)
            throw new ArgumentException("Windows must satisfy 1 <= fast < slow.");
        this.fast = fast;
        this.slow = slow;
    }

    public Signal OnTick(MarketTick tick)
    {
        fastPrices.Enqueue(tick.Price);
        slowPrices.Enqueue(tick.Price);
        fastSum += tick.Price;
        slowSum += tick.Price;
        if (fastPrices.Count > fast)
            fastSum -= fastPrices.Dequeue();
        if (slowPrices.Count > slow)
            slowSum -= slowPrices.Dequeue();
        if (slowPrices.Count < slow)
            return Signal.Hold;

        bool above = fastSum / fast > slowSum / slow;
        bool? previous = wasAbove;
        wasAbove = above;
        if (previous is null || previous == above)
            return Signal.Hold;
        return above ? Signal.Enter : Signal.Exit;
    }
}
