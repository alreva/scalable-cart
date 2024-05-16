using System.Collections.Concurrent;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Persistence;
using CartHost.Cart;
using SharedMessages;

namespace CartHost.CartManager;

public class CartManagerActor : ReceivePersistentActor
{
    private static readonly IActorRef Mediator = DistributedPubSub.Get(Context.System).Mediator;
    private int _nextCartIndex = 1;
    private readonly ConcurrentDictionary<int, IActorRef> _carts = new();

    public CartManagerActor()
    {
        Recover<CartInitialized>(evt =>
        {
            Console.WriteLine($"Recover CartManagerActor: CartInitialized event received for cart {evt.CartId}.");
            Apply(evt);
        });
        
        Command<GetCart>(cmd =>
        {
            var cartId = cmd.CartId;
            Apply(new CartInitialized(cartId));
            Sender.Tell(_carts[cartId]);
        });

        Command<ProductsAddedToCart>(evt =>
        {
            Console.WriteLine($"CartManagerActor: CartInitialized event received for cart {evt.CartId}.");
            Persist(new CartInitialized(evt.CartId), Apply);
        });
        
        SubscribeToCartCreated();
    }

    private void SubscribeToCartCreated()
    {
        Mediator.Tell(new Subscribe(Topics.CartCreated(), Self));
    }

    private void Apply(CartInitialized evt)
    {
        _nextCartIndex = Math.Max(evt.CartId, _nextCartIndex) + 1;
        _carts.GetOrAdd(
            evt.CartId,
            id => System.ActorOf(Props.Create(() => new CartActor(id))));
    }

    public override string PersistenceId => "cart-manager";

    public ActorSystem System => Context.System;
}