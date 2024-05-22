namespace SharedMessages;

public static class IntegrationMessages
{
    public static class E
    {
        public record ProductPriceUpdated(string ProductName, decimal NewPrice);
        public record CartChanged(int CartId, CartMessages.CartDetails Details);
    }
}
