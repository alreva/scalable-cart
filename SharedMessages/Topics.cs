namespace SharedMessages;

public static class Topics
{
    public static Topic ProductPriceUpdated(int productId) => new Topic($"product-price-updated-{productId}");
    public static Topic CartChanged() => new Topic($"cart-changed");
}