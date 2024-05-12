namespace SharedMessages;

public class PriceUpdated
{
    public PriceUpdated(string productName, decimal newPrice)
    {
        ProductName = productName;
        NewPrice = newPrice;
    }

    public string ProductName { get; }
    public decimal NewPrice { get; }
}