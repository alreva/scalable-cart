using CartHost.Marten.Domain;
using Marten;
using CartClientId = (string CartId, string ClientId);
using ProductInCart = (int ProductId, string CartId);

namespace CartHost.Marten.Orchestration;

public class CartRegistry (
    IDocumentStore store,
    IQuerySession querySession
)
{
    private static readonly string Id = WellKnownAggregateIds.CartRegistry;

    public async Task RegisterCartConnected(CartClientId cartClientId)
    {
        await using var session = store.LightweightSession();
        session.Events.Append(Id, new E.CartConnected(cartClientId.ClientId, cartClientId.CartId));
        await session.SaveChangesAsync();
    }

    public async Task RegisterCartDisconnected(string clientId)
    {
        await using var session = store.LightweightSession();
        session.Events.Append(Id, new E.CartDisconnected(clientId));
        await session.SaveChangesAsync();
    }

    public async Task RegisterProductInCart(ProductInCart productInCart)
    {
        await using var session = store.LightweightSession();
        session.Events.Append(Id, new E.ProductInCartRegistered(productInCart.ProductId, productInCart.CartId));
        await session.SaveChangesAsync();
    }
    
    public async Task<string[]> GetCartsWithProduct(int productId)
    {
        var allEvents = await querySession.Events.FetchStreamAsync(Id);
        List<string> result = [];
        foreach (var evt in allEvents)
        {
            if (evt.Data is E.ProductInCartRegistered productInCart && productInCart.ProductId == productId)
            {
                result.Add(productInCart.CartId);
            }
        }
        
        return result.ToArray();
    }

    public async Task<IEnumerable<CartClientId>> GetClientIdsForCarts(params IEnumerable<string> cartIds)
    {
        var allEvents = await querySession.Events.FetchStreamAsync(Id);
        
        List<CartClientId> connectedCarts = [];
        foreach (var evt in allEvents.Select(evt => evt.Data))
        {
            switch (evt)
            {
                case E.CartConnected cartConnected:
                    connectedCarts.Add((cartConnected.CartId, cartConnected.ClientId));
                    break;
                case E.CartDisconnected cartDisconnected:
                {
                    var index = connectedCarts
                        .FindIndex(x => string.Equals(
                            x.ClientId,
                            cartDisconnected.ClientId,
                            StringComparison.OrdinalIgnoreCase));
                    if (index != -1)
                    {
                        connectedCarts.RemoveAt(index);
                    }

                    break;
                }
            }
        }

        return connectedCarts;
    }

    private static class E
    {
        public record CartConnected(string ClientId, string CartId);
        public record CartDisconnected(string ClientId);
        public record ProductInCartRegistered(int ProductId, string CartId);
    }
}