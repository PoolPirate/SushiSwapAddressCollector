namespace SushiSwapAddressCollector.Models;
public class TokenInfo
{
    public string ContractAddress { get; set; }
    public ushort Decimals { get; set; }
    public string? Name { get; set; }
    public string Symbol { get; set; }

    public TokenInfo(string contractAddress, ushort decimals, string? name, string symbol)
    {
        ContractAddress = contractAddress;
        Decimals = decimals;
        Name = name;
        Symbol = symbol;
    }

    public string ToCSV() 
        => Name is null
            ? $"{ContractAddress},{Decimals},null,\"{Symbol}\""
            : $"{ContractAddress},{Decimals},\"{Name}\",\"{Symbol}\"";
}
