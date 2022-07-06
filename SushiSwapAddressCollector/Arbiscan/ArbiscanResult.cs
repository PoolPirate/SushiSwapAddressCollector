namespace SushiSwapAddressCollector.Arbiscan;
public class ArbiscanResult<T>
{
    public T Result { get; set; }

    public ArbiscanResult(T result)
    {
        Result = result;
    }
}
