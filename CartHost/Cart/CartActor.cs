using Akka.Actor;
using Akka.Event;
using Akka.Persistence;
using SharedMessages;

namespace CartHost.Cart;

public class CartActor : ReceivePersistentActor, ILogReceive
{
    private static readonly ILoggingAdapter Logger = Context.GetLogger();
    
    private readonly List<CartMessages.LineItem> _items = new();
    private decimal TotalPrice => _items.Sum(i => i.Price * i.Quantity);

    public CartActor(int id)
    {
        Id = id;
        
        Recover<CartMessages.E.ProductAdded>(evt =>
        {
            Logger.Info(
                "Recover Cart {0}: Product {1} added to cart.",
                Id,
                evt.ProductId);
            Apply(evt);
        });
        Recover<CartMessages.E.PriceUpdated>(evt =>
        {
            Logger.Info(
                "Recover Cart {0}: Product {1} price updated to {2}.",
                Id, evt.ProductId, evt.NewPrice);
            Apply(evt);
        });
        
        Command<CartMessages.Q.GetCartDetails>(_ =>
        {
            Logger.Info("Cart {0}: Querying cart details", Id);
            Sender.Tell(BuildDetails());
        });
        
        Command<CartMessages.C.AddProduct>(cmd =>
        { 
            Logger.Info("Cart {0}: Adding product {1} to cart.", Id, cmd.Id);
            Self.Tell(new CartMessages.E.ProductAdded(cmd.Id, cmd.Price));
        });
        Command<CartMessages.C.UpdatePrice>(cmd =>
        {
            Logger.Info("Cart {0}: Updating product {1} price to {2}.", Id, cmd.ProductId, cmd.NewPrice);
            Self.Tell(new CartMessages.E.PriceUpdated(cmd.ProductId, cmd.NewPrice));
        });
        
        Command<CartMessages.E.ProductAdded>(e =>
        {
            Persist(e, evt =>
            {
                Apply(evt);
                Logger.Info("Cart {0}: Product {1} added to cart.", Id, evt.ProductId);
                Context.Parent.Tell(new IntegrationMessages.E.CartChanged(Id, BuildDetails()));
            });
        });
        Command<CartMessages.E.PriceUpdated>(e =>
        {
            Persist(e, evt =>
            {
                Apply(evt);
                Logger.Info("Cart {0}: Product {1} price updated to {2}", Id, evt.ProductId, evt.NewPrice);
                Context.Parent.Tell(new IntegrationMessages.E.CartChanged(Id, BuildDetails()));
            });
        });
    }

    private CartMessages.CartDetails BuildDetails()
    {
        return new CartMessages.CartDetails(Id, _items.ToArray(), TotalPrice);
    }

    private int Id { get; }
    
    public override string PersistenceId => $"cart_{Id}";

    private void Apply(CartMessages.E.ProductAdded evt)
    {
        var existing = _items.Find(i => i.ProductId == evt.ProductId);
        if (existing != null)
        {
            existing.Quantity++;
        }
        else
        {
            _items.Add(new CartMessages.LineItem(evt.ProductId, evt.ProductPrice, 1));
        }
    }

    private void Apply(CartMessages.E.PriceUpdated evt)
    {
        var existing = _items.Find(i => i.ProductId == evt.ProductId);
        if (existing != null)
        {
            existing.Price = evt.NewPrice;
        }
        // well, even though this is not normal, ignore the update...
    }
}