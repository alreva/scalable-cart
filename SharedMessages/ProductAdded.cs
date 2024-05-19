namespace SharedMessages;

public record ProductAdded(string ProductName, decimal ProductPrice);
public record ProductAddedToCart(int CartId, string ProductName, decimal ProductPrice);

