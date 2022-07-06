using SushiSwapAddressCollector.Collectors;

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
            Collector = new SwapPairAddressCollector();
            break;
        case "2":
            Collector = new KashiPairAddressCollector();
            break;
        default:
            Console.WriteLine("Unknown Input, Type 1 or 2");
            break;
    }
}

await Collector.CollectAsync();
Console.WriteLine("Completed! Press a key to exit...");
Console.ReadKey();
