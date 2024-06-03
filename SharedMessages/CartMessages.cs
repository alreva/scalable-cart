using Orleans;

namespace SharedMessages;

public partial class CartMessages
{
    public class C
    {
        public record UpdatePrice(int ProductId, decimal NewPrice);
        
        [GenerateSerializer, Immutable]
        public record AddProduct(int Id, decimal Price);
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
        public record ProductAdded(int ProductId, decimal ProductPrice);
        public record PriceUpdated(int ProductId, decimal NewPrice);
    }
    
    [GenerateSerializer, Immutable]
    public record CartDetails(int CartId, LineItem[] LineItems, decimal TotalPrice)
    {
        public static CartDetails Query(int cartId) => new(cartId, [], 0);
    }
    
    [GenerateSerializer]
    public class LineItem(int productId, decimal price, int quantity)
    {
        public int ProductId { get; init; } = productId;
        public decimal Price { get; set; } = price;
        public int Quantity { get; set; } = quantity;

        public void Deconstruct(out int productId, out decimal price, out int quantity)
        {
            productId = this.ProductId;
            price = this.Price;
            quantity = this.Quantity;
        }
    }
}