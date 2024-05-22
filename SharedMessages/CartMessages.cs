namespace SharedMessages;

public partial class CartMessages
{
    public class C
    {
        public record UpdatePrice(string ProductName, decimal NewPrice);
        public record AddProduct(string Name, decimal Price);
    }
    
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

    public record CartDetails(int CartId, LineItem[] LineItems, decimal TotalPrice)
    {
        public static CartDetails Query(int cartId) => new(cartId, [], 0);
    }
    
    public class LineItem(string productName, decimal price, int quantity)
    {
        public string ProductName { get; init; } = productName;
        public decimal Price { get; set; } = price;
        public int Quantity { get; set; } = quantity;

        public void Deconstruct(out string ProductName, out decimal Price, out int Quantity)
        {
            ProductName = this.ProductName;
            Price = this.Price;
            Quantity = this.Quantity;
        }
    }
}