using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace SushiSwapAddressCollector.Contracts;
public class KashiLendingPair
{
    [Function("asset", "address")]
    public class AssetFunction : FunctionMessage
    {
    }

    [Function("collateral", "address")]
    public class CollateralFunction : FunctionMessage
    {
    }
}
