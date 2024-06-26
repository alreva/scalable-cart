using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Persistence;
using SharedMessages;

namespace Cart;

public class CartActor : ReceivePersistentActor
{
    private static readonly IActorRef Mediator = DistributedPubSub.Get(Context.System).Mediator;
    private readonly List<LineItem> _items = new();
    private decimal TotalPrice => _items.Sum(i => i.Price * i.Quantity);

    public CartActor(int id)
    {
        Id = id;
        
        Recover<ProductAdded>(product =>
        {
            Console.WriteLine($"Recover: Product {product.ProductName} added to cart {Id}");
            HandleProductAdded(product);
            SubscribeToProductUpdates(product.ProductName);
        });
        Recover<ProductPriceUpdated>(price =>
        {
            Console.WriteLine($"Recover: Product {price.ProductName} price updated to {price.NewPrice}");
            HandlePriceUpdated(price);
        });
        
        Command<AddProduct>(addProduct =>
        {
            Console.WriteLine($"Adding product {addProduct.Name} to cart {Id}");
            Self.Tell(new ProductAdded(addProduct.Name, addProduct.Price, Get));
        });
        
        Command<ProductAdded>(productAdded =>
        {
            Persist(productAdded, p =>
            {
                HandleProductAdded(p);
                SubscribeToProductUpdates(p.ProductName);
                Console.WriteLine($"Product {p.ProductName} added to cart {Id}");
            });
        });
        
        Command<ProductPriceUpdated>(priceUpdated =>
        {
            Persist(priceUpdated, price =>
            {
                HandlePriceUpdated(price);
                Console.WriteLine($"Cart {Id}: Product {price.ProductName} price updated to {price.NewPrice}");
            });
        });
        
        Command<CartDetails>(q =>
        {
            Console.WriteLine($"Cart {Id}: Querying cart details");
            Sender.Tell(GetDetails(q));
        });
    }

    private CartDetails GetDetails(CartDetails q)
    {
        return new CartDetails {
            CartId = q.CartId,
            LineItems = _items.ToArray(),
            TotalPrice = TotalPrice
        };
    }

    public int Id { get; }
    public override string PersistenceId => $"cart_{Id}";

    private void HandleProductAdded(ProductAdded update)
    {
        var existing = _items.Find(i => i.ProductName == update.ProductName);
        if (existing != null)
        {
            existing.Quantity++;
        }
        else
        {
            _items.Add(new LineItem
            {
                ProductName = update.ProductName,
                Price = update.ProductPrice,
                Quantity = 1
            });
        }
    }

    private void HandlePriceUpdated(ProductPriceUpdated update)
    {
        var existing = _items.Find(i => i.ProductName == update.ProductName);
        if (existing != null)
        {
            existing.Price = update.NewPrice;
        }
        // well, even though this is not normal, ignore the update...
    }

    private void SubscribeToProductUpdates(string productName)
    {
        Mediator.Tell(new Subscribe(Topics.ProductPriceUpdated(productName), Self));
    }
}