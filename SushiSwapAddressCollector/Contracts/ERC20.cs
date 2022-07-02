using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace SushiSwapAddressCollector.Contracts;
public class ERC20
{
    [Function("decimals", "uint8")]
    public class DecimalsFunction : FunctionMessage
    {
    }

    [Function("name", "string")]
    public class NameFunction : FunctionMessage
    {
    }

    [Function("symbol", "string")]
    public class SymbolFunction : FunctionMessage
    {
    }
}
