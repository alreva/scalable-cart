namespace SharedMessages;

public class ProductAdded
{
    public ProductAdded(string productName, decimal productPrice)
    {
        ProductName = productName;
        ProductPrice = productPrice;
    }

    public string ProductName { get; set; }
    public decimal ProductPrice { get; set; }

}