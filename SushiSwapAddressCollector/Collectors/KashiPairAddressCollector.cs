using Common.Services;
using Microsoft.Extensions.Logging;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.JsonRpc.Client;
using Nethereum.Web3;
using SushiSwapAddressCollector.ApiClient;
using SushiSwapAddressCollector.Configuration;
using SushiSwapAddressCollector.Contracts;
using SushiSwapAddressCollector.Extensions;
using SushiSwapAddressCollector.Models;
using System.Text.Json;

namespace SushiSwapAddressCollector.Collectors;
public class KashiPairAddressCollector : Singleton, ICollector
{
    public const string KashiDeployTopic0 = "0xd62166f3c2149208e51788b1401cc356bf5da1fc6c7886a32e18570f57d88b3b";

    [Inject]
    private readonly Web3 Web3 = null!;
    [Inject]
    private readonly EVMExplorerClient Arbiscan = null!;
    [Inject]
    private readonly SushiSwapOptions SushiSwapOptions = null!;

    private IContractQueryHandler<KashiLendingPair.AssetFunction> AssetQueryHandler = null!;
    private IContractQueryHandler<KashiLendingPair.CollateralFunction> CollateralQueryHandler = null!;

    private IContractQueryHandler<ERC20.DecimalsFunction> DecimalsQueryHandler = null!;
    private IContractQueryHandler<ERC20.NameFunction> NameQueryHandler = null!;
    private IContractQueryHandler<ERC20.SymbolFunction> SymbolQueryHandler = null!;

    private readonly Dictionary<string, TokenInfo> Tokens = new Dictionary<string, TokenInfo>();

    protected override ValueTask InitializeAsync()
    {
        AssetQueryHandler = Web3.Eth.GetContractQueryHandler<KashiLendingPair.AssetFunction>();
        CollateralQueryHandler = Web3.Eth.GetContractQueryHandler<KashiLendingPair.CollateralFunction>();

        DecimalsQueryHandler = Web3.Eth.GetContractQueryHandler<ERC20.DecimalsFunction>();
        NameQueryHandler = Web3.Eth.GetContractQueryHandler<ERC20.NameFunction>();
        SymbolQueryHandler = Web3.Eth.GetContractQueryHandler<ERC20.SymbolFunction>();

        return ValueTask.CompletedTask;
    }

    public async Task CollectAsync()
    {
        string[] kashiAddresses = await GetKashiPairAddressesAsync();
        Logger.LogInformation("Found {addressCount} Kashi Pair contracts", kashiAddresses.Length);

        var kashiPairs = new List<KashiPairInfo>();

        foreach (string kashiAddress in kashiAddresses)
        {
            if (kashiAddress.ToLower() == SushiSwapOptions.KashiMasterContractAddress.ToLower())
            {
                Logger.LogInformation("Skipping {kashiAddress}, is MasterContract", kashiAddress);
                continue;
            }

            Logger.LogInformation("Loading Kashi Pair Info for {kashiAddress}", kashiAddress);
            var pairInfo = await GetPairInfoAsync(kashiAddress);
            kashiPairs.Add(pairInfo);
        }

        Logger.LogInformation("Writing CSV output to {outputFilePath}", Path.GetFullPath("SushiKashiPairs.csv"));
        await File.WriteAllLinesAsync(Path.GetFullPath("SushiKashiPairs.csv"), kashiPairs.Select(x => x.ToCSV()));

        Logger.LogInformation("Writing Json output to {outputFilePath}", Path.GetFullPath("SushiKashiPairs.json"));
        await File.WriteAllTextAsync(Path.GetFullPath("SushiKashiPairs.json"), JsonSerializer.Serialize(kashiPairs));
    }

    private async Task<string[]> GetKashiPairAddressesAsync()
    {
        var events = await Arbiscan.GetPastEventsAsync(KashiDeployTopic0);
        return events
            .Where(x =>
            {
                string addressComponent = x.Topics[1][26..];
                string address = $"0x{addressComponent}".ToLower();
                return address == SushiSwapOptions.KashiMasterContractAddress.ToLower();
            })
            .Select(x =>
            {
                string addressComponent = x.Topics[2][26..];
                return $"0x{addressComponent}";
            })
            .ToArray();
    }

    private async Task<KashiPairInfo> GetPairInfoAsync(string pairAddress)
    {
        string assetAddress = await AssetQueryHandler!.QuerySafeAsync<KashiLendingPair.AssetFunction, string>(pairAddress);
        string collateralAddress = await CollateralQueryHandler!.QuerySafeAsync<KashiLendingPair.CollateralFunction, string>(pairAddress);

        var asset = await GetTokenInfoAsync(assetAddress);
        var collateral = await GetTokenInfoAsync(collateralAddress);

        return new KashiPairInfo(pairAddress, asset, collateral);
    }

    private async Task<TokenInfo> GetTokenInfoAsync(string tokenAddress)
    {
        if (Tokens!.TryGetValue(tokenAddress, out var tokenInfo))
        {
            return tokenInfo;
        }

        ushort decimals = await DecimalsQueryHandler!.QuerySafeAsync<ERC20.DecimalsFunction, ushort>(tokenAddress);
        string symbol = await SymbolQueryHandler!.QuerySafeAsync<ERC20.SymbolFunction, string>(tokenAddress);

        string? name = null;

        try
        {
            name = await NameQueryHandler!.QuerySafeAsync<ERC20.NameFunction, string>(tokenAddress);
        }
        catch (RpcResponseException)
        {
            Logger.LogCritical("Missing 'name' function on ERC-20 Contract at address {address}", tokenAddress);
        }

        tokenInfo = new TokenInfo(tokenAddress, decimals, name, symbol);
        Tokens.Add(tokenAddress, tokenInfo);

        return tokenInfo;
    }
}
