namespace SharedMessages;

public static class ProductManagerMessages
{
    public static class C
    {
        public record UpdateProductPrice(string ProductName, decimal NewPrice);
    }
}