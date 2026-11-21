# Program 4 Community

Run repeatable, offline backtests on historical market ticks with C# and .NET 10. The package includes a small simulation library, a command-line example and synthetic market data.

## Quick start

Install the .NET 10 SDK and run these commands from the repository root.

```sh
dotnet build Program4.Community.slnx --configuration Release
dotnet run --project src/Program4.Cli --configuration Release --no-build -- samples/ticks.csv DEMO
dotnet run --project tests/Program4.Tests --configuration Release --no-build
```

The CLI prints a JSON summary with starting and ending cash, return, maximum drawdown, simulated fill count and fees. Pass two additional positive integers to change the fast and slow moving-average windows. The defaults are 2 and 4 ticks.

## Data format

The CSV header is `timestamp,symbol,bid,ask,price`. Use the round-trip .NET timestamp format, for example `2026-01-01T00:00:00.0000000+00:00`, and decimal points. Fields must be unquoted. Rows for the selected symbol must have strictly increasing timestamps and positive prices, with bid at or below ask. The sample symbol `DEMO` is synthetic.

## Simulation

Each strategy signal can fill from the next tick's quote. Entries use ask plus slippage and exits use bid minus slippage. The simulation holds up to one long position and closes it on the final tick. It applies fees to both sides. During replay, drawdown uses cash plus holdings marked at bid. The example uses a fixed trade notional and simulates full fills. It is intended for research and education.

To test a different signal, implement `IHistoricalSignalStrategy` and pass a fresh instance to `Backtest.Run`. The library accepts `BacktestOptions` for capital, trade size, fees and slippage. There are no external package dependencies.

## Contributing

When opening an issue, include the .NET SDK version, reproduction commands and a synthetic input example. The test harness runs as a console application and returns a nonzero exit code on failure. Please run it before opening a pull request.
