using Nethereum.Contracts;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.JsonRpc.Client;
using System.Net;

namespace SushiSwapAddressCollector.Extensions;
public static class Extensions
{
    public static async Task<T> QuerySafeAsync<TMessage, T>(this IContractQueryHandler<TMessage> queryHandler, string contractAddress, TMessage message = null!)
        where TMessage : FunctionMessage, new()
    {
        while (true)
        {
            try
            {
                return await queryHandler.QueryAsync<T>(contractAddress, message);
            }
            catch (RpcClientUnknownException e) when (e.InnerException is HttpRequestException ex)
            {
                if (ex.StatusCode != HttpStatusCode.TooManyRequests)
                {
                    throw;
                }

                Console.WriteLine("Rate Limit Hit, Waiting 10 Seconds...");
                await Task.Delay(10000);
            }
        }
    }
}
