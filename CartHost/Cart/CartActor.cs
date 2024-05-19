using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Event;
using Akka.Persistence;
using SharedMessages;

namespace CartHost.Cart;

public class CartActor : ReceivePersistentActor, ILogReceive
{
    private static readonly ILoggingAdapter Logger = Context.GetLogger();
    
    private readonly List<LineItem> _items = new();
    private decimal TotalPrice => _items.Sum(i => i.Price * i.Quantity);

    public CartActor(int id)
    {
        Id = id;
        
        Recover<ProductAdded>(evt =>
        {
            Logger.Info(
                "Recover Cart {0}: Product {1} added to cart.",
                Id,
                evt.ProductName);
            Apply(evt);
        });
        Recover<ProductPriceUpdated>(evt =>
        {
            Logger.Info(
                "Recover Cart {0}: Product {1} price updated to {2}.",
                Id,
                evt.ProductName,
                evt.NewPrice);
            Apply(evt);
        });
        
        Command<GetCartDetails>(_ =>
        {
            Logger.Info("Cart {0}: Querying cart details", Id);
            Sender.Tell(new CartDetails {
                CartId = Id,
                LineItems = _items.ToArray(),
                TotalPrice = TotalPrice
            });
        });
        
        Command<AddProduct>(cmd =>
        { 
            Logger.Info("Cart {0}: Adding product {1} to cart.", Id, cmd.Name);
            Self.Tell(new ProductAdded(cmd.Name, cmd.Price));
        });
        Command<UpdateProductPrice>(cmd =>
        {
            Logger.Info("Cart {0}: Updating product {1} price to {2}.", Id, cmd.ProductName, cmd.NewPrice);
            Self.Tell(new ProductPriceUpdated(cmd.ProductName, cmd.NewPrice));
        });
        
        Command<ProductAdded>(e =>
        {
            Persist(e, evt =>
            {
                Apply(evt);
                Logger.Info("Cart {0}: Product {1} added to cart.", Id, evt.ProductName);
                Context.Parent.Tell(new ProductAddedToCart(Id, evt.ProductName, evt.ProductPrice));
            });
        });
        Command<ProductPriceUpdated>(e =>
        {
            Persist(e, evt =>
            {
                Apply(evt);
                Logger.Info("Cart {0}: Product {1} price updated to {2}", Id, evt.ProductName, evt.NewPrice);
            });
        });
    }

    private int Id { get; }
    
    public override string PersistenceId => $"cart_{Id}";

    private void Apply(ProductAdded evt)
    {
        var existing = _items.Find(i => i.ProductName == evt.ProductName);
        if (existing != null)
        {
            existing.Quantity++;
        }
        else
        {
            _items.Add(new LineItem
            {
                ProductName = evt.ProductName,
                Price = evt.ProductPrice,
                Quantity = 1
            });
        }
    }

    private void Apply(ProductPriceUpdated evt)
    {
        var existing = _items.Find(i => i.ProductName == evt.ProductName);
        if (existing != null)
        {
            existing.Price = evt.NewPrice;
        }
        // well, even though this is not normal, ignore the update...
    }
}