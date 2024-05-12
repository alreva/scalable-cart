namespace SharedMessages;

public class CartDetails
{
    public static CartDetails Query(string cartId) => new()
    {
        CartId = cartId,
        LineItems = [],
        TotalPrice = 0
    };
    
    public required string CartId { get; init; }
    public required LineItem[] LineItems { get; init; } = [];
    public required decimal TotalPrice { get; set; } = 0;
}