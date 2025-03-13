using Marten;
using ProductPrice = (int ProductId, decimal Price);
using ProductInCart = (int ProductId, string CartId);
using CartClientId = (string CartId, string ClientId);

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
        var notificationTasks = clientIds.Select(SendNotificationForCart);
        await Task.WhenAll(notificationTasks);
    }

    private async Task SendNotificationForCart(CartClientId cartClientId)
    {
        var cart = await querySession.Events.AggregateStreamAsync<Domain.Cart.Cart>(cartClientId.CartId);
        if (cart is not null)
        {
            await cartNotifier.SendNotification(cartClientId.ClientId, cart.GetDetails());
        }
    }
}