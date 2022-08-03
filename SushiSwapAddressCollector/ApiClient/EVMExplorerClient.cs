using Common.Services;
using SushiSwapAddressCollector.Configuration;
using SushiSwapAddressCollector.Models;
using System.Net.Http.Json;

namespace SushiSwapAddressCollector.ApiClient;
public class EVMExplorerClient : Singleton
{
    [Inject]
    private readonly ExplorerOptions ExplorerOptions = null!;
    [Inject]
    private readonly HttpClient Client = null!;

    public async Task<TransactionEvent[]> GetPastEventsAsync(string topic0)
    {
        string url = $"{ExplorerOptions.ApiURL}?module=logs&action=getLogs&topic0={topic0}&apikey={ExplorerOptions.ApiKey}";
        var result = await Client.GetFromJsonAsync<EVMExplorerResult<TransactionEvent[]>>(url);
        return result!.Result;
    }
}
