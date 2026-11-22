# Program 4 Community

[Try the interactive fill explorer](https://aminenayatollahi-source.github.io/program4-community/) — select an order to see its signal and execution, or change a sample price and replay the backtest in your browser.

Run repeatable, offline backtests on historical market ticks with C# and .NET 10. The package includes a small simulation library, a command-line example and synthetic market data.

## Run the packaged demo

Download the archive for your system. Both x64 packages include the .NET runtime and synthetic tick data.

| System | Download | Command |
| --- | --- | --- |
| Windows x64 (PowerShell) | [ZIP](https://github.com/aminenayatollahi-source/program4-community/releases/download/v0.1.1/program4-community-v0.1.1-win-x64.zip) | `.\Program4.Cli.exe samples\ticks.csv DEMO` |
| Linux x64 | [tar.gz](https://github.com/aminenayatollahi-source/program4-community/releases/download/v0.1.1/program4-community-v0.1.1-linux-x64.tar.gz) | `./Program4.Cli samples/ticks.csv DEMO` |

Extract the archive, open a terminal in the folder containing `QUICKSTART.txt`, and run the command above. Look for `TickCount` = 16, `FillCount` = 4 and `EndingCash` of about 99777.66 in the JSON output. To try your own CSV, replace the sample path and symbol in the same command.

[Report your first run](https://github.com/aminenayatollahi-source/program4-community/issues/new?template=first-run-feedback.yml)

<a name="quick-start"></a>

## Build from source

Install the .NET 10 SDK. Clone this repository or download the source archive from the [release page](https://github.com/aminenayatollahi-source/program4-community/releases), then run these commands from the source directory.

```sh
dotnet build Program4.Community.slnx --configuration Release
dotnet run --project src/Program4.Cli --configuration Release --no-build -- samples/ticks.csv DEMO
dotnet run --project tests/Program4.Tests --configuration Release --no-build
```

The CLI prints a JSON summary with starting and ending cash, return, maximum drawdown, simulated fill count and fees. Pass two additional positive integers to change the fast and slow moving-average windows. The defaults are 2 and 4 ticks.

## Run your own CSV

Save the following example as `my-ticks.csv` in the repository root, or use tick data you have permission to use. Replace `ABC` in the command with an exact symbol from your CSV.

```csv
timestamp,symbol,bid,ask,price
2026-01-01T00:00:00Z,ABC,99.9,100.1,100
2026-01-01T00:01:00Z,ABC,100.9,101.1,101
```

```sh
dotnet run --project src/Program4.Cli --configuration Release --no-build -- my-ticks.csv ABC 2 4
```

The two-row example should report `TickCount: 2` and `FillCount: 0`. It demonstrates the input format. Use a longer sequence to exercise moving-average signals.

## Data format

The CSV header is `timestamp,symbol,bid,ask,price`. Use ISO 8601 timestamps with seconds and an explicit offset. `2026-01-01T00:00:00Z`, `2026-01-01T03:30:00+03:30`, and the .NET round-trip format are supported. Fractional seconds are optional and can have up to seven digits. Use decimal points and unquoted fields. If a row cannot be parsed, the CLI reports its line number and the invalid field. Rows for the selected symbol must have strictly increasing timestamps and positive prices, with bid at or below ask. The sample symbol `DEMO` is synthetic.

## Simulation

Each strategy signal can fill from the next tick's quote. Entries use ask plus slippage and exits use bid minus slippage. The simulation holds up to one long position and closes it on the final tick. It applies fees to both sides. During replay, drawdown uses cash plus holdings marked at bid. The example uses a fixed trade notional and simulates full fills. It is intended for research and education.

To test a different signal, implement `IHistoricalSignalStrategy` and pass a fresh instance to `Backtest.Run`. The library accepts `BacktestOptions` for capital, trade size, fees and slippage. There are no external package dependencies.

## Worked example: a next-tick fill

An entry signal arrives at a price of 100. The next quote is 120, so a trade of 240 buys two units. On the final tick, the simulator sells both at 150. With a 1% fee on each side and zero slippage, the entry fee is 2.40 and the exit fee is 3.00. Starting with 1000, the account ends with `1000 - 240 - 2.40 + 300 - 3.00 = 1054.60`. A fill at the signal price would buy a different quantity.

Run the [C# example and its assertions](tests/Program4.Tests/Program.cs) from the source directory with the .NET 10 SDK:

```sh
dotnet run --project tests/Program4.Tests --configuration Release
```

The example prints the fill count, total fees and ending cash calculated by the simulation. The test checks all three values.

## Contributing

If you try the example or run a backtest with your own CSV, share what worked and where you got stuck using the [first-run feedback form](https://github.com/aminenayatollahi-source/program4-community/issues/new?template=first-run-feedback.yml). Include your operating system, whether you used a package or built from source, and what happened. For input problems, use a synthetic example you can share publicly. The test harness runs as a console application and returns a nonzero exit code on failure. Please run it before opening a pull request.
