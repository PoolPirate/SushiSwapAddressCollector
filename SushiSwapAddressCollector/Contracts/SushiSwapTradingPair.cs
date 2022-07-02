using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace SushiSwapAddressCollector.Contracts;

public class SushiSwapTradingPair
{
    [Function("token0", "address")]
    public class Token0Function : FunctionMessage
    {
    }

    [Function("token1", "address")]
    public class Token1Function : FunctionMessage
    {
    }
}