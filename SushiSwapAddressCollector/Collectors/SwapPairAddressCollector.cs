using Nethereum.Contracts.ContractHandlers;
using Nethereum.Web3;
using SushiSwapAddressCollector.Contracts;
using SushiSwapAddressCollector.Extensions;
using SushiSwapAddressCollector.Models;
using System.Text.Json;

namespace SushiSwapAddressCollector.Collectors;
public class SwapPairAddressCollector : ICollector
{
    private readonly Web3 Web3;

    private readonly IContractQueryHandler<SushiSwapFactory.AllPairsLengthFunction> PairCountQueryHandler;
    private readonly IContractQueryHandler<SushiSwapFactory.AllPairsFunction> PairAddressQueryHandler;
    
    private readonly IContractQueryHandler<SushiSwapTradingPair.Token0Function> Token0QueryHandler;
    private readonly IContractQueryHandler<SushiSwapTradingPair.Token1Function> Token1QueryHandler;

    private readonly IContractQueryHandler<ERC20.DecimalsFunction> DecimalsQueryHandler;
    private readonly IContractQueryHandler<ERC20.NameFunction> NameQueryHandler;
    private readonly IContractQueryHandler<ERC20.SymbolFunction> SymbolQueryHandler;

    private readonly Dictionary<string, TokenInfo> Tokens;

    public SwapPairAddressCollector()
    {
        Web3 = new Web3(Constants.RPCUrl);

        PairCountQueryHandler = Web3.Eth.GetContractQueryHandler<SushiSwapFactory.AllPairsLengthFunction>();
        PairAddressQueryHandler = Web3.Eth.GetContractQueryHandler<SushiSwapFactory.AllPairsFunction>();

        Token0QueryHandler = Web3.Eth.GetContractQueryHandler<SushiSwapTradingPair.Token0Function>();
        Token1QueryHandler = Web3.Eth.GetContractQueryHandler<SushiSwapTradingPair.Token1Function>();

        DecimalsQueryHandler = Web3.Eth.GetContractQueryHandler<ERC20.DecimalsFunction>();
        NameQueryHandler = Web3.Eth.GetContractQueryHandler<ERC20.NameFunction>();
        SymbolQueryHandler = Web3.Eth.GetContractQueryHandler<ERC20.SymbolFunction>();

        Tokens = new Dictionary<string, TokenInfo>();
    }

    public async Task CollectAsync()
    {
        ulong pairCount = await PairCountQueryHandler.QueryAsync<ulong>(Constants.FactoryAddress);

        Console.WriteLine($"Found {pairCount} unique trading pairs!");

        var pairAddresses = new List<SwapPairInfo>();

        for (ulong i = 0; i < pairCount; i++)
        {
            Console.WriteLine($"Querying pair address for pair index {i}");

            var pairQuery = new SushiSwapFactory.AllPairsFunction()
            {
                Index = i
            };

            string pairAddress = await PairAddressQueryHandler.QuerySafeAsync<SushiSwapFactory.AllPairsFunction, string>(Constants.FactoryAddress, pairQuery);
            Console.WriteLine($"Found pair contract at address {pairAddress}");

            var pairInfo = await GetPairInfoAsync(pairAddress);
            pairAddresses.Add(pairInfo);
        }

        Console.WriteLine($"Writing CSV output to {Path.GetFullPath("SushiSwapPairs.csv")}");
        await File.WriteAllLinesAsync(Path.GetFullPath("SushiSwapPairs.csv"), pairAddresses.Select(x => x.ToCSV()));

        Console.WriteLine($"Writing Json output to {Path.GetFullPath("SushiSwapPairs.json")}");
        await File.WriteAllTextAsync(Path.GetFullPath("SushiSwapPairs.json"), JsonSerializer.Serialize(pairAddresses));
    }

    private async Task<SwapPairInfo> GetPairInfoAsync(string pairAddress)
    {
        string token0Address = await Token0QueryHandler!.QuerySafeAsync<SushiSwapTradingPair.Token0Function, string>(pairAddress);
        string token1Address = await Token1QueryHandler!.QuerySafeAsync<SushiSwapTradingPair.Token1Function, string>(pairAddress);

        var token0 = await GetTokenInfoAsync(token0Address);
        var token1 = await GetTokenInfoAsync(token1Address);

        return new SwapPairInfo(pairAddress, token0, token1);
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
