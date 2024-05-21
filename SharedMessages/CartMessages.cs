namespace SharedMessages;

public partial class CartMessages
{
    public class Q
    {
        public record GetCartDetails
        {
            public static GetCartDetails Instance { get; } = new();
        }
    }

    public class E
    {
        public record CartDetailsReceived(CartDetails Data);
        public record ProductAdded(string ProductName, decimal ProductPrice);
        public record PriceUpdated(string ProductName, decimal NewPrice);
    }

    public record CartDetails(int CarttId, LineItem[] LineItems, decimal TotalPrice)
    {
        public static CartDetails Query(int cartId) => new(cartId, [], 0);
    }
}