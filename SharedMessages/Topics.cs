namespace SharedMessages;

public static class Topics
{
    public static Topic ProductPriceUpdated(string productName) => new Topic($"product-price-updated-{productName}");
    public static Topic CartChanged() => new Topic($"cart-changed");
}