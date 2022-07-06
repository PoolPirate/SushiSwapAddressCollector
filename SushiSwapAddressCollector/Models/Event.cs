namespace SushiSwapAddressCollector.Models;
public class TransactionEvent
{
    public string Address { get; set; }
    public string[] Topics { get; set; }

    public TransactionEvent(string address, string[] topics)
    {
        Address = address;
        Topics = topics;
    }
}
