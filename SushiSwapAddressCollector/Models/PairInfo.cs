namespace SushiSwapAddressCollector.Models;
public class PairInfo
{
    public string ContractAddress { get; set; }

    public TokenInfo Token0 { get; set; }
    public TokenInfo Token1 { get; set; }

    public PairInfo(string contractAddress, TokenInfo token0, TokenInfo token1)
    {
        ContractAddress = contractAddress;
        Token0 = token0;
        Token1 = token1;
    }

    public string ToCSV() 
        => $"{ContractAddress},{Token0.ToCSV()},{Token1.ToCSV()}";
}
