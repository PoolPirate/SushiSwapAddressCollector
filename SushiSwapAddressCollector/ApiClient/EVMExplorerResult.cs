namespace SushiSwapAddressCollector.ApiClient;
public class EVMExplorerResult<T>
{
    public T Result { get; set; }

    public EVMExplorerResult(T result)
    {
        Result = result;
    }
}
