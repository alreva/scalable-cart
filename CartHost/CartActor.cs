using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Persistence;
using SharedMessages;

namespace CartHost;

public class CartActor : ReceivePersistentActor
{
    private static readonly IActorRef Mediator = DistributedPubSub.Get(Context.System).Mediator;
    private bool _hasProducts;
    private readonly List<LineItem> _items = new();
    private decimal TotalPrice => _items.Sum(i => i.Price * i.Quantity);

    public CartActor(int id)
    {
        Id = id;
        
        Recover<ProductAdded>(product =>
        {
            Console.WriteLine($"Recover Cart {Id}: Product {product.ProductName} added to cart {Id}");
            Apply(product);
            SubscribeToProductUpdates(product.ProductName);
        });
        Recover<ProductPriceUpdated>(price =>
        {
            Console.WriteLine($"Recover Cart {Id}: Product {price.ProductName} price updated to {price.NewPrice}");
            Apply(price);
        });
        
        Command<GetCartDetails>(_ =>
        {
            Console.WriteLine($"Cart {Id}: Querying cart details");
            Sender.Tell(new CartDetails {
                CartId = Id,
                LineItems = _items.ToArray(),
                TotalPrice = TotalPrice
            });
        });
        
        Command<AddProduct>(addProduct =>
        {
            Console.WriteLine($"Cart {Id}: Adding product {addProduct.Name} to cart {Id}");
            Self.Tell(new ProductAdded(addProduct.Name, addProduct.Price));
        });
        Command<ProductAdded>(productAdded =>
        {
            Persist(productAdded, p =>
            {
                var oldHasProducts = _hasProducts;
                Apply(p);
                SubscribeToProductUpdates(p.ProductName);
                if (!oldHasProducts)
                {
                    NotifyProductAddedToCart();
                }
                Console.WriteLine($"Cart {Id}: Product {p.ProductName} added to cart {Id}");
            });
        });
        Command<ProductPriceUpdated>(priceUpdated =>
        {
            Persist(priceUpdated, price =>
            {
                Apply(price);
                Console.WriteLine($"Cart {Id}: Product {price.ProductName} price updated to {price.NewPrice}");
            });
        });
    }

    public int Id { get; }
    
    public override string PersistenceId => $"cart_{Id}";

    private void NotifyProductAddedToCart()
    {
        Mediator.Tell(new Publish(Topics.CartCreated(), new ProductsAddedToCart(Id)));
    }

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
        _hasProducts = true;
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

    private void SubscribeToProductUpdates(string productName)
    {
        Mediator.Tell(new Subscribe(Topics.ProductPriceUpdated(productName), Self));
    }
}