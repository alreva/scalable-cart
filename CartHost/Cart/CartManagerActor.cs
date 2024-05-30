using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Event;
using Akka.Persistence;
using SharedMessages;

namespace CartHost.Cart;

public class CartManagerActor : ReceivePersistentActor, ILogReceive
{
    private static readonly IActorRef Mediator = DistributedPubSub.Get(Context.System).Mediator;
    private static readonly ILoggingAdapter Logger = Context.GetLogger();
    private readonly HashSet<int> _productSubscriptions = new();
    private readonly Dictionary<int, HashSet<int>> _subscribedCarts = new();
    private int _evtCount;

    public CartManagerActor()
    {
        Recover<IntegrationMessages.E.CartChanged>(evt =>
        {
            Logger.Info("Recover: cart {0} changed.", evt.CartId);
            Apply(evt, true);
        });
        Recover<IntegrationMessages.E.ProductPriceUpdated>(evt =>
        {
            Logger.Info("Recover: product {0} price updated to {1}.", evt.ProductId, evt.NewPrice);
            Apply(evt, true);
        });
        Recover<SnapshotOffer>(offer =>
        {
            Logger.Info("Recover: snapshot offer.");
            var state = offer.Snapshot as State;
            state?.FillState(this);
        });
        
        Command<CartManagerMessages.GetCart>(cmd =>
        {
            var cartId = cmd.CartId;
            Sender.Tell(GetOrCreateCart(cartId));
        });

        Command<IntegrationMessages.E.CartChanged>(e =>
        {
            Persist(e, evt => Apply(evt, false));
            MakeSnapshotIfNeeded();
        });
        Command<IntegrationMessages.E.ProductPriceUpdated>(e =>
        {
            Persist(e, evt =>  Apply(evt, false));
            MakeSnapshotIfNeeded();
        });
        
        Command<SaveSnapshotSuccess>(_ =>
        {
            Logger.Info("Save snapshot success.");
        });
    }

    private void Apply(IntegrationMessages.E.CartChanged evt, bool recovering)
    {
        Logger.Info("CartManagerActor: cart {0} changed.", evt.CartId);
        SubscribeToProductUpdates(evt);
        if (!recovering)
        {
            Mediator.Tell(new Publish(Topics.CartChanged(), evt));
        }
    }
    
    private void Apply(IntegrationMessages.E.ProductPriceUpdated evt, bool recovering)
    {
        if (!recovering)
        {
            var productId = evt.ProductId;
            if (!_productSubscriptions.Contains(productId))
            {
                Logger.Info("CartManagerActor: not subscribed to product updates for product {0}.", productId);
                return;
            }
            var subscribedCarts = _subscribedCarts[productId];
            foreach (var cartId in subscribedCarts)
            {
                Logger.Info("CartManagerActor: updating product price for product {0} in cart {1}.", productId, cartId);
                var cart = GetOrCreateCart(cartId);
                cart.Tell(new CartMessages.C.UpdatePrice(productId, evt.NewPrice));
            }
        }
    }

    private static IActorRef GetOrCreateCart(int cartId)
    {
        var actorName = $"cart-{cartId}";
        var cart = Context.Child(actorName);
        
        if (!cart.IsNobody())
        {
            return cart;
        }
        
        return Context.ActorOf(Props.Create(() => new CartActor(cartId)), actorName);
    }
    
    private void SubscribeToProductUpdates(IntegrationMessages.E.CartChanged evt)
    {
        foreach (var productId in evt.Details.LineItems.Select(i => i.ProductId))
        {
            SubscribeToSingleProductUpdates(evt.CartId, productId);
        }
    }

    private void SubscribeToSingleProductUpdates(int cartId, int productId)
    {
        if (_productSubscriptions.Contains(productId))
        {
            Logger.Info("CartManagerActor: already subscribed to product updates for product {0}.", productId);
            var subscribedCarts = _subscribedCarts[productId];
            if (subscribedCarts.Contains(cartId))
            {
                Logger.Info("CartManagerActor: cart {0} already subscribed to product updates for product {1}.", cartId, productId);
                return;
            }
            Logger.Info("CartManagerActor: subscribing cart {0} to product updates for product {1}.", cartId, productId);
            subscribedCarts.Add(cartId);
            return;
        }
        Logger.Info("CartManagerActor: subscribing to product updates for product {0}.", productId);
        Logger.Info("CartManagerActor: subscribing cart {0} to product updates for product {1}.", cartId, productId);
        Mediator.Tell(new Subscribe(Topics.ProductPriceUpdated(productId), Self));
        _productSubscriptions.Add(productId);
        _subscribedCarts[productId] = [cartId];
    }

    private void MakeSnapshotIfNeeded()
    {
        _evtCount++;
        if (_evtCount % 50 == 0)
        {
            Logger.Info("CartManagerActor: saving snapshot, since collected {0} events.", _evtCount);
            SaveSnapshot(State.GetState(this));
        }
    }

    public override string PersistenceId => "cart-manager";
    
    public class State
    {
        public required CartProducts[] SubscribedCarts { get; init; }
        
        public static State GetState(CartManagerActor actor)
        {
            var subscriptions = actor
                ._subscribedCarts.Select(
                    kvp => new CartProducts
                    {
                        ProductId = kvp.Key,
                        CartIds = kvp.Value.ToArray()
                    })
                .ToArray();
            return new State
            {
                SubscribedCarts = subscriptions
            };
        }

        public void FillState(CartManagerActor actor)
        {
            actor._productSubscriptions.Clear();
            actor._subscribedCarts.Clear();
            foreach (var (productId, cartIds) in SubscribedCarts)
            {
                foreach (var cartId in cartIds)
                {
                    actor.SubscribeToSingleProductUpdates(cartId, productId);
                }
            }
        }
        
        public class CartProducts
        {
            public required int ProductId { get; set; }
            public required int[] CartIds { get; set; }

            public void Deconstruct(out int productId, out int[] cartIds)
            {
                productId = ProductId;
                cartIds = CartIds;
            }
        }
    }
}