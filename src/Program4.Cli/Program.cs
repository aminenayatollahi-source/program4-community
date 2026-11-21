using System.Globalization;
using System.Text.Json;
using Program4.Backtesting;

if (args is ["--help"] || args.Length == 0)
{
    Console.WriteLine("Usage: Program4.Cli <ticks.csv> <symbol> [fast slow]");
    return args.Length == 0 ? 2 : 0;
}

int fast = 2;
int slow = 4;
if (args.Length is not (2 or 4) ||
    (args.Length == 4 && (!int.TryParse(args[2], NumberStyles.None, CultureInfo.InvariantCulture, out fast) ||
                          !int.TryParse(args[3], NumberStyles.None, CultureInfo.InvariantCulture, out slow))))
{
    Console.Error.WriteLine("Usage: Program4.Cli <ticks.csv> <symbol> [fast slow]");
    return 2;
}

try
{
    using StreamReader input = File.OpenText(args[0]);
    IReadOnlyList<MarketTick> ticks = TickCsv.Read(input, args[1]);
    BacktestResult result = Backtest.Run(ticks, new MovingAverageCross(fast, slow), new BacktestOptions());
    Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
    return 0;
}
catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or FormatException or OverflowException)
{
    Console.Error.WriteLine(ex.Message);
    return 2;
}
