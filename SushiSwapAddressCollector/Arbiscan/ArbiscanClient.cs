using SushiSwapAddressCollector.Models;
using System.Net.Http.Json;

namespace SushiSwapAddressCollector.Arbiscan;
public class ArbiscanClient
{
    private const string ApiURL = "https://api.arbiscan.io/api";
    private readonly string APIKey;
    private readonly HttpClient Client;

    public ArbiscanClient(string apiKey)
    {
        APIKey = apiKey;
        Client = new HttpClient();
    }

    public async Task<TransactionEvent[]> GetPastEventsAsync(string topic0)
    {
        string url = $"{ApiURL}?module=logs&action=getLogs&topic0={topic0}&apikey={APIKey}";
        var result = await Client.GetFromJsonAsync<ArbiscanResult<TransactionEvent[]>>(url);
        return result!.Result;
    }
}
