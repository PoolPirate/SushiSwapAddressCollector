using Nethereum.Contracts.ContractHandlers;
using Nethereum.Web3;
using SushiSwapAddressCollector.Arbiscan;
using SushiSwapAddressCollector.Contracts;
using SushiSwapAddressCollector.Extensions;
using SushiSwapAddressCollector.Models;
using System.Text.Json;

namespace SushiSwapAddressCollector.Collectors;
public class KashiPairAddressCollector : ICollector
{
    private readonly Web3 Web3;
    private readonly ArbiscanClient Arbiscan;

    private readonly IContractQueryHandler<KashiLendingPair.AssetFunction> AssetQueryHandler;
    private readonly IContractQueryHandler<KashiLendingPair.CollateralFunction> CollateralQueryHandler;

    private readonly IContractQueryHandler<ERC20.DecimalsFunction> DecimalsQueryHandler;
    private readonly IContractQueryHandler<ERC20.NameFunction> NameQueryHandler;
    private readonly IContractQueryHandler<ERC20.SymbolFunction> SymbolQueryHandler;

    private readonly Dictionary<string, TokenInfo> Tokens;

    public KashiPairAddressCollector()
    {
        Web3 = new Web3(Constants.RPCUrl);
        Arbiscan = new ArbiscanClient(Constants.ArbiscanAPIKey);

        AssetQueryHandler = Web3.Eth.GetContractQueryHandler<KashiLendingPair.AssetFunction>();
        CollateralQueryHandler = Web3.Eth.GetContractQueryHandler<KashiLendingPair.CollateralFunction>();

        DecimalsQueryHandler = Web3.Eth.GetContractQueryHandler<ERC20.DecimalsFunction>();
        NameQueryHandler = Web3.Eth.GetContractQueryHandler<ERC20.NameFunction>();
        SymbolQueryHandler = Web3.Eth.GetContractQueryHandler<ERC20.SymbolFunction>();

        Tokens = new Dictionary<string, TokenInfo>();
    }

    public async Task CollectAsync()
    {
        string[] kashiAddresses = await GetKashiPairAddressesAsync();
        Console.WriteLine($"Found {kashiAddresses.Length} Kashi Pair contracts");

        var kashiPairs = new List<KashiPairInfo>();

        foreach (string kashiAddress in kashiAddresses)
        {
            if (kashiAddress.ToLower() == Constants.KashiMasterContractAddress.ToLower())
            {
                Console.WriteLine($"Skipping {kashiAddress}, is MasterContract");
                continue;
            }

            Console.WriteLine($"Loading Kashi Pair Info for {kashiAddress}");
            var pairInfo = await GetPairInfoAsync(kashiAddress);
            kashiPairs.Add(pairInfo);
        }

        Console.WriteLine($"Writing CSV output to {Path.GetFullPath("SushiKashiPairs.csv")}");
        await File.WriteAllLinesAsync(Path.GetFullPath("SushiKashiPairs.csv"), kashiPairs.Select(x => x.ToCSV()));

        Console.WriteLine($"Writing Json output to {Path.GetFullPath("SushiKashiPairs.json")}");
        await File.WriteAllTextAsync(Path.GetFullPath("SushiKashiPairs.json"), JsonSerializer.Serialize(kashiPairs));
    }

    private async Task<string[]> GetKashiPairAddressesAsync()
    {
        var events = await Arbiscan.GetPastEventsAsync(Constants.KashiDeployTopic0);
        return events
            .Where(x =>
            {
                string addressComponent = x.Topics[1][26..];
                string address = $"0x{addressComponent}".ToLower();
                return address == Constants.KashiMasterContractAddress.ToLower();
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
        string name = await NameQueryHandler!.QuerySafeAsync<ERC20.NameFunction, string>(tokenAddress);
        string symbol = await SymbolQueryHandler!.QuerySafeAsync<ERC20.SymbolFunction, string>(tokenAddress);

        tokenInfo = new TokenInfo(tokenAddress, decimals, name, symbol);
        Tokens.Add(tokenAddress, tokenInfo);

        return tokenInfo;
    }
}
