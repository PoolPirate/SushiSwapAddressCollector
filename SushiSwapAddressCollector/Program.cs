using SushiSwapAddressCollector.Contracts;
using SushiSwapAddressCollector.Extensions;
using SushiSwapAddressCollector.Models;
using System.Text.Json;

const string FactoryAddress = "0xc35DADB65012eC5796536bD9864eD8773aBc74C4";
const string RPCUrl = "https://arb1.arbitrum.io/rpc";

var web3 = new Nethereum.Web3.Web3(RPCUrl);

var pairCountQueryHandler = web3.Eth.GetContractQueryHandler<SushiSwapFactory.AllPairsLengthFunction>();
var pairAddressQueryHandler = web3.Eth.GetContractQueryHandler<SushiSwapFactory.AllPairsFunction>();

var token0QueryHandler = web3.Eth.GetContractQueryHandler<SushiSwapTradingPair.Token0Function>();
var token1QueryHandler = web3.Eth.GetContractQueryHandler<SushiSwapTradingPair.Token1Function>();

var decimalsQueryHandler = web3.Eth.GetContractQueryHandler<ERC20.DecimalsFunction>();
var nameQueryHandler = web3.Eth.GetContractQueryHandler<ERC20.NameFunction>();
var symbolQueryHandler = web3.Eth.GetContractQueryHandler<ERC20.SymbolFunction>();

ulong pairCount = await pairCountQueryHandler.QueryAsync<ulong>(FactoryAddress);

Console.WriteLine($"Found {pairCount} unique trading pairs!");

var tokens = new Dictionary<string, TokenInfo>(); 
var pairAddresses = new List<PairInfo>();

for(ulong i = 0; i < pairCount; i++)
{
    Console.WriteLine($"Querying pair address for pair index {i}");

    var pairQuery = new SushiSwapFactory.AllPairsFunction()
    {
        Index = i
    };

    string pairAddress = await pairAddressQueryHandler.QuerySafeAsync<SushiSwapFactory.AllPairsFunction, string>(FactoryAddress, pairQuery);
    Console.WriteLine($"Found pair contract at address {pairAddress}");

    var pairInfo = await GetPairInfoAsync(pairAddress);
    pairAddresses.Add(pairInfo);
}

string json = JsonSerializer.Serialize(pairAddresses);

Console.WriteLine($"Writing CSV output to {Path.GetFullPath("SushiPair.csv")}");
await File.WriteAllLinesAsync(Path.GetFullPath("SushiPair.csv"), pairAddresses.Select(x => x.ToCSV()));

Console.WriteLine($"Writing Json output to {Path.GetFullPath("SushiPair.json")}");
await File.WriteAllTextAsync(Path.GetFullPath("SushiPair.json"), json);

async Task<PairInfo> GetPairInfoAsync(string pairAddress)
{
    string token0Address = await token0QueryHandler!.QuerySafeAsync<SushiSwapTradingPair.Token0Function, string>(pairAddress);
    string token1Address = await token1QueryHandler!.QuerySafeAsync<SushiSwapTradingPair.Token1Function, string>(pairAddress);

    var token0 = await GetTokenInfoAsync(token0Address);
    var token1 = await GetTokenInfoAsync(token1Address);

    return new PairInfo(pairAddress, token0, token1);
}

async Task<TokenInfo> GetTokenInfoAsync(string tokenAddress)
{
    if (tokens!.TryGetValue(tokenAddress, out var tokenInfo))
    {
        return tokenInfo;
    }

    ushort decimals = await decimalsQueryHandler!.QuerySafeAsync<ERC20.DecimalsFunction, ushort>(tokenAddress);
    string name = await nameQueryHandler!.QuerySafeAsync<ERC20.NameFunction, string>(tokenAddress);
    string symbol = await symbolQueryHandler!.QuerySafeAsync<ERC20.SymbolFunction, string>(tokenAddress);

    tokenInfo = new TokenInfo(tokenAddress, decimals, name, symbol);
    tokens.Add(tokenAddress, tokenInfo);

    return tokenInfo;
}

Console.WriteLine("Completed! Press a key to exit...");
Console.ReadKey();