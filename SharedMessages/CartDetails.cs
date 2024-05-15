namespace SharedMessages;

public class CartDetails
{
    public static CartDetails Query(int cartId) => new()
    {
        CartId = cartId,
        LineItems = [],
        TotalPrice = 0
    };
    
    public required int CartId { get; init; }
    public required LineItem[] LineItems { get; init; } = [];
    public required decimal TotalPrice { get; set; } = 0;
}

public record GetCartDetails
{
    public static GetCartDetails Instance { get; } = new();
}

public record CartDetailsReceived(CartDetails Data);