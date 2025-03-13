namespace CartHost.Marten.Domain.Cart;

public class Cart
{
    public string Id { get; set; }
    
    public List<M.LineItem> LineItems { get; set; } = new();

    public decimal TotalPrice { get; set; }
    
    public M.CartDetails GetDetails()
    {
        return new(
            Id,
            LineItems.ToArray(),
            TotalPrice
        );
    }
    
    public void Apply(E.ProductAdded e)
    {
        var index = LineItems.FindIndex(x => x.ProductId == e.ProductId);
        if (index >= 0)
        {
            var existingItem = LineItems[index];
            LineItems[index] = existingItem with { Quantity = existingItem.Quantity + 1 };
        }
        else
        {
            LineItems.Add(new (e.ProductId, e.Price, 1));
        }

        TotalPrice += e.Price;
    }
    
    public void Apply(E.ProductPriceUpdated e)
    {
        var index = LineItems.FindIndex(x => x.ProductId == e.ProductId);
        if (index >= 0)
        {
            var existingItem = LineItems[index];
            var qty = existingItem.Quantity;
            var newItem = existingItem with { Price = e.Price };
            LineItems[index] = newItem;
            TotalPrice += (newItem.Price - existingItem.Price) * qty;
        }
    }
    
    public static class M
    {
        public record CartDetails(string CartId, LineItem[] LineItems, decimal TotalPrice);
        public record LineItem(int ProductId, decimal Price, int Quantity);
    }
    
    public static class E
    {
        public record ProductAdded(int ProductId, decimal Price);
        public record ProductPriceUpdated(int ProductId, decimal Price);
    }
}