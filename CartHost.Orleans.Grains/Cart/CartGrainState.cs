using SharedMessages;

namespace CartHost.Orleans.Grains.Cart;

internal record CartGrainState
{
    public List<CartMessages.LineItem> LineItems { get; init; } = [];
    public decimal TotalPrice { get; init; } = 0;

    public void Deconstruct(out List<CartMessages.LineItem> lineItems, out decimal totalPrice)
    {
        lineItems = LineItems;
        totalPrice = TotalPrice;
    }
}