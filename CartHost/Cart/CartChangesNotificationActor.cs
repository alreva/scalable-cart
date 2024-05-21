using System.Collections.Concurrent;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Event;
using Microsoft.AspNetCore.SignalR;
using SharedMessages;

namespace CartHost.Cart;

public class CartChangesNotificationActor : ReceiveActor
{
    private static readonly IActorRef Mediator = DistributedPubSub.Get(Context.System).Mediator;
    private static readonly IHubContext<CartHub> CartHub = HubContext.Cart;
    private static readonly ILoggingAdapter Logger = Context.GetLogger();
    private readonly ConcurrentDictionary<int, ConcurrentBag<string>> _cartConnections = new();
    private readonly ConcurrentDictionary<string, ConcurrentBag<int>> _connectionCarts = new();

    public CartChangesNotificationActor()
    {
        Mediator.Tell(new Subscribe(Topics.CartChanged(), Self));

        Receive<ClientCartConnected>(Apply);
        Receive<ClientCartDisconnected>(Apply);
        Receive<IntegrationMessages.CartChanged>(Apply);
    }

    private void Apply(ClientCartConnected evt)
    {
        Logger.Info("CartChangesNotificationActor: Client {0} connected cart {1}", evt.ConnectionId, evt.CartId);
        var cartId = evt.CartId;
        var connectionId = evt.ConnectionId;
        var cartConnections = _cartConnections
            .GetOrAdd(cartId, []);
        cartConnections.Add(connectionId);
        var connectionCarts = _connectionCarts
            .GetOrAdd(connectionId, []);
        connectionCarts.Add(cartId);
    }

    private void Apply(ClientCartDisconnected evt)
    {
        Logger.Info("CartChangesNotificationActor: Client {0} disconnected", evt.ConnectionId);
        var connectionId = evt.ConnectionId;
        if (!_connectionCarts.TryGetValue(connectionId, out var cartIds))
        {
            return;
        }
        
        foreach (var cartId in cartIds)
        {
            if (_cartConnections.TryGetValue(cartId, out var cartConnections))
            {
                cartConnections.TryTake(out var _);
            }
        }
        _connectionCarts.TryRemove(connectionId, out var _);
    }

    private async void Apply(IntegrationMessages.CartChanged evt)
    {
        Logger.Info("CartChangesNotificationActor: Cart {0} changed", evt.CartId);
        var cartId = evt.CartId;
        if (!_cartConnections.TryGetValue(cartId, out var connectionIds))
        {
            Logger.Info("CartChangesNotificationActor: No connections for cart {0}", cartId);
            return;
        }
        
        Logger.Info("CartChangesNotificationActor: Sending message to {0} connections for cart {1}", connectionIds.Count, cartId);
        await CartHub.Clients
            .Clients(connectionIds)
            .SendAsync("ReceiveMessage", evt);
    }
}