using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace SushiSwapAddressCollector.Contracts;

public class SushiSwapFactory
{
    [Function("allPairsLength", "uint256")]
    public class AllPairsLengthFunction : FunctionMessage
    {
    }

    [Function("allPairs", "address")]
    public class AllPairsFunction : FunctionMessage
    {
        [Parameter("uint256", "input")]
        public ulong Index { get; set; }
    }
}
