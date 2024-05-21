namespace SharedMessages;

public class IntegrationMessages
{
    public record ProductPriceUpdated(string ProductName, decimal NewPrice);
    public record CartChanged(int CartId, CartMessages.CartDetails Details);
}
