using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Event;
using Akka.Persistence;
using CartHost.Cart;
using SharedMessages;

namespace CartHost.CartManager;

public class CartManagerActor : ReceivePersistentActor, ILogReceive
{
    private static readonly IActorRef Mediator = DistributedPubSub.Get(Context.System).Mediator;
    private static readonly ILoggingAdapter Logger = Context.GetLogger();
    private readonly HashSet<string> _productSubscriptions = new();
    private readonly Dictionary<string, HashSet<int>> _subscribedCarts = new();

    public CartManagerActor()
    {
        Recover<IntegrationMessages.CartChanged>(evt =>
        {
            Logger.Info("Recover: cart {0} changed.", evt.CartId);
            Apply(evt);
        });
        Recover<IntegrationMessages.ProductPriceUpdated>(evt =>
        {
            Logger.Info("Recover: product {0} price updated to {1}.", evt.ProductName, evt.NewPrice);
            Apply(evt, true);
        });
        
        Command<GetCart>(cmd =>
        {
            var cartId = cmd.CartId;
            Sender.Tell(GetOrCreateCart(cartId));
        });

        Command<IntegrationMessages.CartChanged>(evt =>
        {
            Persist(evt, Apply);
        });
        Command<IntegrationMessages.ProductPriceUpdated>(e =>
        {
            Persist(e, evt =>  Apply(evt, false));
        });
    }

    private void Apply(IntegrationMessages.CartChanged evt)
    {
        Logger.Info("CartManagerActor: cart {0} changed.", evt.CartId);
        SubscribeToProductUpdates(evt);
        Mediator.Tell(new Publish(Topics.CartChanged(), evt));
    }
    
    private void Apply(IntegrationMessages.ProductPriceUpdated evt, bool recovering)
    {
        if (!recovering)
        {
            var productName = evt.ProductName;
            if (!_productSubscriptions.Contains(productName))
            {
                Logger.Info("CartManagerActor: not subscribed to product updates for product {0}.", productName);
                return;
            }
            var subscribedCarts = _subscribedCarts[productName];
            foreach (var cartId in subscribedCarts)
            {
                Logger.Info("CartManagerActor: updating product price for product {0} in cart {1}.", productName, cartId);
                var cart = GetOrCreateCart(cartId);
                cart.Tell(new UpdateProductPrice(productName, evt.NewPrice));
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
    
    private void SubscribeToProductUpdates(IntegrationMessages.CartChanged evt)
    {
        foreach (var productName in evt.Details.LineItems.Select(i => i.ProductName))
        {
            var cartId = evt.CartId;
            if (_productSubscriptions.Contains(productName))
            {
                Logger.Info("CartManagerActor: already subscribed to product updates for product {0}.", productName);
                var subscribedCarts = _subscribedCarts[productName];
                if (subscribedCarts.Contains(cartId))
                {
                    Logger.Info("CartManagerActor: cart {0} already subscribed to product updates for product {1}.", cartId, productName);
                    return;
                }
                Logger.Info("CartManagerActor: subscribing cart {0} to product updates for product {1}.", cartId, productName);
                subscribedCarts.Add(cartId);
                return;
            }
            Logger.Info("CartManagerActor: subscribing to product updates for product {0}.", productName);
            Logger.Info("CartManagerActor: subscribing cart {0} to product updates for product {1}.", cartId, productName);
            Mediator.Tell(new Subscribe(Topics.ProductPriceUpdated(productName), Self));
            _productSubscriptions.Add(productName);
            _subscribedCarts[productName] = [cartId];
        }
    }

    public override string PersistenceId => "cart-manager";
}