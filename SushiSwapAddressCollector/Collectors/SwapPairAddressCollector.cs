using Common.Services;
using Microsoft.Extensions.Logging;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.JsonRpc.Client;
using Nethereum.Web3;
using SushiSwapAddressCollector.Configuration;
using SushiSwapAddressCollector.Contracts;
using SushiSwapAddressCollector.Extensions;
using SushiSwapAddressCollector.Models;
using System.Text.Json;

namespace SushiSwapAddressCollector.Collectors;
public class SwapPairAddressCollector : Singleton, ICollector
{
    [Inject]
    private readonly Web3 Web3 = null!;
    [Inject]
    private readonly SushiSwapOptions SushiSwapOptions = null!;

    private IContractQueryHandler<SushiSwapFactory.AllPairsLengthFunction> PairCountQueryHandler = null!;
    private IContractQueryHandler<SushiSwapFactory.AllPairsFunction> PairAddressQueryHandler = null!;

    private IContractQueryHandler<SushiSwapTradingPair.Token0Function> Token0QueryHandler = null!;
    private IContractQueryHandler<SushiSwapTradingPair.Token1Function> Token1QueryHandler = null!;

    private IContractQueryHandler<ERC20.DecimalsFunction> DecimalsQueryHandler = null!;
    private IContractQueryHandler<ERC20.NameFunction> NameQueryHandler = null!;
    private IContractQueryHandler<ERC20.SymbolFunction> SymbolQueryHandler = null!;

    private readonly Dictionary<string, TokenInfo> Tokens = new Dictionary<string, TokenInfo>();

    protected override ValueTask InitializeAsync()
    {
        PairCountQueryHandler = Web3.Eth.GetContractQueryHandler<SushiSwapFactory.AllPairsLengthFunction>();
        PairAddressQueryHandler = Web3.Eth.GetContractQueryHandler<SushiSwapFactory.AllPairsFunction>();

        Token0QueryHandler = Web3.Eth.GetContractQueryHandler<SushiSwapTradingPair.Token0Function>();
        Token1QueryHandler = Web3.Eth.GetContractQueryHandler<SushiSwapTradingPair.Token1Function>();

        DecimalsQueryHandler = Web3.Eth.GetContractQueryHandler<ERC20.DecimalsFunction>();
        NameQueryHandler = Web3.Eth.GetContractQueryHandler<ERC20.NameFunction>();
        SymbolQueryHandler = Web3.Eth.GetContractQueryHandler<ERC20.SymbolFunction>();

        return ValueTask.CompletedTask;
    }

    public async Task CollectAsync()
    {
        ulong pairCount = await PairCountQueryHandler.QueryAsync<ulong>(SushiSwapOptions.FactoryAddress);

        Logger.LogInformation("Found {pairCount} unique trading pairs!", pairCount);

        var pairAddresses = new List<SwapPairInfo>();

        for (ulong i = 0; i < pairCount; i++)
        {
            Logger.LogInformation("Querying pair address for pair index {index}", i);

            var pairQuery = new SushiSwapFactory.AllPairsFunction()
            {
                Index = i
            };

            string pairAddress = await PairAddressQueryHandler.QuerySafeAsync<SushiSwapFactory.AllPairsFunction, string>(SushiSwapOptions.FactoryAddress, pairQuery);
            Logger.LogInformation("Found pair contract at address {pairAddress}", pairAddress);

            var pairInfo = await GetPairInfoAsync(pairAddress);
            pairAddresses.Add(pairInfo);
        }

        Logger.LogInformation("Writing CSV output to {outputFilePath}", Path.GetFullPath("SushiSwapPairs.csv"));
        await File.WriteAllLinesAsync(Path.GetFullPath("SushiSwapPairs.csv"), pairAddresses.Select(x => x.ToCSV()));

        Logger.LogInformation("Writing Json output to {outputFilePath}", Path.GetFullPath("SushiSwapPairs.json"));
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
