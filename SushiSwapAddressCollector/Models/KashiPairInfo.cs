namespace SushiSwapAddressCollector.Models;
public class KashiPairInfo
{
    public string ContractAddress { get; set; }

    public TokenInfo Asset { get; set; }
    public TokenInfo Collateral { get; set; }

    public KashiPairInfo(string contractAddress, TokenInfo asset, TokenInfo collateral)
    {
        ContractAddress = contractAddress;
        Asset = asset;
        Collateral = collateral;
    }

    public string ToCSV()
        => $"{ContractAddress},{Asset.ToCSV()},{Collateral.ToCSV()}";
}
