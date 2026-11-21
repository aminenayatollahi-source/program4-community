# Program 4 Community

Run repeatable, offline backtests on historical market ticks with C# and .NET 10. The package includes a small simulation library, a command-line example and synthetic market data.

## Quick start

Install the .NET 10 SDK. Clone this repository or download the source archive from the [release page](https://github.com/aminenayatollahi-source/program4-community/releases), then run these commands from the source directory.

```sh
dotnet build Program4.Community.slnx --configuration Release
dotnet run --project src/Program4.Cli --configuration Release --no-build -- samples/ticks.csv DEMO
dotnet run --project tests/Program4.Tests --configuration Release --no-build
```

The CLI prints a JSON summary with starting and ending cash, return, maximum drawdown, simulated fill count and fees. Pass two additional positive integers to change the fast and slow moving-average windows. The defaults are 2 and 4 ticks.

The bundled `DEMO` run should report `TickCount: 16`, `FillCount: 4`, and ending cash of about `99777.66`. Running the same command again should return identical JSON.

## Run your own CSV

Save the following example as `my-ticks.csv` in the repository root, or use tick data you have permission to use. Replace `ABC` in the command with an exact symbol from your CSV.

```csv
timestamp,symbol,bid,ask,price
2026-01-01T00:00:00.0000000+00:00,ABC,99.9,100.1,100
2026-01-01T00:01:00.0000000+00:00,ABC,100.9,101.1,101
```

```sh
dotnet run --project src/Program4.Cli --configuration Release --no-build -- my-ticks.csv ABC 2 4
```

The two-row example should report `TickCount: 2` and `FillCount: 0`. It demonstrates the input format. Use a longer sequence to exercise moving-average signals.

## Data format

The CSV header is `timestamp,symbol,bid,ask,price`. Use the round-trip .NET timestamp format, for example `2026-01-01T00:00:00.0000000+00:00`, and decimal points. Fields must be unquoted. Rows for the selected symbol must have strictly increasing timestamps and positive prices, with bid at or below ask. The sample symbol `DEMO` is synthetic.

## Simulation

Each strategy signal can fill from the next tick's quote. Entries use ask plus slippage and exits use bid minus slippage. The simulation holds up to one long position and closes it on the final tick. It applies fees to both sides. During replay, drawdown uses cash plus holdings marked at bid. The example uses a fixed trade notional and simulates full fills. It is intended for research and education.

To test a different signal, implement `IHistoricalSignalStrategy` and pass a fresh instance to `Backtest.Run`. The library accepts `BacktestOptions` for capital, trade size, fees and slippage. There are no external package dependencies.

## Contributing

If you try the example or run a backtest with your own CSV, share what worked and where you got stuck using the [first-run feedback form](https://github.com/aminenayatollahi-source/program4-community/issues/new?template=first-run-feedback.yml). Include your OS, .NET SDK version and the command you ran. For input problems, use a synthetic example you can share publicly. The test harness runs as a console application and returns a nonzero exit code on failure. Please run it before opening a pull request.
