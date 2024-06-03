using SharedMessages;

namespace CartHost.Orleans.Grains.Cart;

public interface ICartGrain : IGrainWithIntegerKey
{
    [Alias("GetDetails")]
    Task<CartMessages.CartDetails> GetDetails();
    
    [Alias("Add")]
    Task Add(CartMessages.C.AddProduct cmd);

    [Alias("UpdateProductPrice")]
    Task UpdateProductPrice(int productId, decimal price);
}