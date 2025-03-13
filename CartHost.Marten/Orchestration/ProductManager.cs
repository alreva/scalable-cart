using Marten;
using ProductPrice = (int ProductId, decimal Price);
using ProductInCart = (int ProductId, string CartId);

namespace CartHost.Marten.Orchestration;

public class ProductManager(
    IDocumentStore store,
    IQuerySession querySession,
    CartRegistry cartRegistry,
    CartHubNotifier cartNotifier)
{
    public async Task UpdateProductPrice(ProductPrice productPrice)
    {
        var carts = await cartRegistry.GetCartsWithProduct(productPrice.ProductId);
        await UpdateCarts(productPrice, carts);
        await SendNotifications(carts);
    }

    public async Task RegisterProductInCart(ProductInCart productInCart)
    {
        await cartRegistry.RegisterProductInCart(productInCart);
        await SendNotifications(productInCart.CartId);
    }

    private async Task UpdateCarts(ProductPrice productPrice, string[] cartIds)
    {
        await using var session = store.LightweightSession();
        foreach (var cartId in cartIds)
        {
            session.Events.Append(
                cartId,
                new Domain.Cart.Cart.E.ProductPriceUpdated(productPrice.ProductId, productPrice.Price));
        }
        await session.SaveChangesAsync();
    }

    private async Task SendNotifications(params string[] cartIds)
    {
        var clientIds = await cartRegistry.GetClientIdsForCarts(cartIds);
        List<Task> notificationTasks = [];
        foreach (var (cartId, clientId) in clientIds)
        {
            notificationTasks.Add(ProcessCart());
            continue;

            async Task ProcessCart()
            {
                var cart = await querySession.Events.AggregateStreamAsync<Domain.Cart.Cart>(cartId);
                if (cart is not null)
                {
                    await cartNotifier.SendNotification(clientId, cart.GetDetails());
                }
            }
        }
        await Task.WhenAll(notificationTasks);
    }
}