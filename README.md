# SushiSwapAddressCollector

## Features
- Identify Sushiswap Kashi & Swap Pair addresses on Arbitrum (Other chains with minor modifications possible)
- Exports contract address, token addresses, token name, symbol, decimals in both JSON and CSV

## Requirements
- [.NET6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

## Build & Run
- Configure app by creating a `appsettings.json` file in the `SushiSwapAddressCollector` directory
  - Instead of manually creating it you can copy over configurations from the `SushiSwapAddressCollector/ExampleConfigurations` directory
  - If you do make sure to rename the file to `appsettings.json`
- `dotnet run` (from root directory)


# Execution Outputs
- Kashi Pairs:
https://gist.github.com/PoolPirate/0aee64379fdf5e0191da492eba5679ff

- Swap Pairs:
https://gist.github.com/PoolPirate/c3121e4a76e6632b6698657ebd1edac6
