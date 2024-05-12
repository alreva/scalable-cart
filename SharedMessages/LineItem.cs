namespace SharedMessages;

public class LineItem
{
    public required string ProductName { get; init; }
    public required decimal Price { get; set; }
    public required int Quantity { get; set; }
}