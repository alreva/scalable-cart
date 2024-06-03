using CartHost.Orleans.Grains.Cart;

namespace CartHost.Orleans.Grains.ProductManager;

internal class ProductManagerGrain(IGrainFactory grains)
    : Grain<ProductManagerGrainState>, IProductManagerGrain
{
    public async Task SubscribeCartToProductUpdates(int cartId, int productId)
    {
        State.CartProducts.Add((cartId, productId));
        await WriteStateAsync();
    }

    public async Task UpdateProductPrice(int productId, decimal price)
    {
        var productGrain = grains.GetGrain<IProductGrain>(productId);
        await productGrain.UpdatePrice(price);
        var subscribedCarts = State
            .CartProducts
            .Where(x => x.productId == productId)
            .Select(x => x.cartid);

        foreach (var cartId in subscribedCarts)
        {
            await grains.GetGrain<ICartGrain>(cartId).UpdateProductPrice(productId, price);
        }
    }
}