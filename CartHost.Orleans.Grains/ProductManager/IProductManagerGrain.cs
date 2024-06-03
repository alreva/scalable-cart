namespace CartHost.Orleans.Grains.ProductManager;

public interface IProductManagerGrain : IGrainWithStringKey
{
    Task SubscribeCartToProductUpdates(int cartId, int productId);
    Task UpdateProductPrice(int productId, decimal price);
}