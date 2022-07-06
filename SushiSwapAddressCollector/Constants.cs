namespace SushiSwapAddressCollector;
public class Constants
{
    public const string FactoryAddress = "0xc35DADB65012eC5796536bD9864eD8773aBc74C4";
    public const string RPCUrl = "https://arb1.arbitrum.io/rpc";
    public static readonly string ArbiscanAPIKey = Environment.GetEnvironmentVariable("Arbiscan-ApiKey") ?? throw new InvalidOperationException("Missing Arbiscan API Key! Add to Arbiscan-ApiKey environment variable.");
    public const string KashiDeployTopic0 = "0xd62166f3c2149208e51788b1401cc356bf5da1fc6c7886a32e18570f57d88b3b";
    public const string KashiMasterContractAddress = "0xa010eE0226cd071BeBd8919A1F675cAE1f1f5D3e";
}
