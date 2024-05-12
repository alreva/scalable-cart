namespace SharedMessages;

public class AddProduct(string name, decimal price)
{
    public string Name { get; } = name;
    public decimal Price { get; } = price;
}