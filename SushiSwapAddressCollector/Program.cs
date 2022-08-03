using Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nethereum.Web3;
using SushiSwapAddressCollector.Collectors;
using SushiSwapAddressCollector.Configuration;
using System.Reflection;

var provider = MakeProvider();
await provider.InitializeApplicationAsync(Assembly.GetExecutingAssembly());
provider.RunApplication(Assembly.GetExecutingAssembly());

Console.WriteLine("What should be collected?");
Console.WriteLine("1) SwapPairs");
Console.WriteLine("2) KashiPairs");

ICollector? Collector = null;

while (Collector is null)
{
    string? input = Console.ReadLine();

    switch (input)
    {
        case "1":
            Collector = provider.GetRequiredService<SwapPairAddressCollector>();
            break;
        case "2":
            Collector = provider.GetRequiredService<KashiPairAddressCollector>();
            break;
        default:
            Console.WriteLine("Unknown Input, Type 1 or 2");
            break;
    }
}

await Collector.CollectAsync();
Console.WriteLine("Completed! Press a key to exit...");
Console.ReadKey();

IServiceProvider MakeProvider()
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", false)
        .Build();

    return new ServiceCollection()
        .AddSingleton<HttpClient>()
        .AddSingleton(configuration)
        .AddApplication(configuration, Assembly.GetExecutingAssembly())
        .AddLogging(options => options.AddConsole())
        .AddSingleton(provider => new Web3(provider.GetRequiredService<ChainOptions>().NodeRPCUrl))
        .BuildServiceProvider();
}