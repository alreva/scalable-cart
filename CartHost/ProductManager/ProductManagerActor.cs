using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Event;
using Akka.Persistence;
using CartHost.ProductManager.Catalog;
using SharedMessages;

namespace CartHost.ProductManager;

public class ProductManagerActor: ReceivePersistentActor, ILogReceive
{ 
    private static readonly IActorRef Mediator = DistributedPubSub.Get(Context.System).Mediator;
    private static readonly ILoggingAdapter Logger = Context.GetLogger();
    public CatalogLoader CatalogLoader { get; }
    private readonly Dictionary<int, decimal> _productPrices = new();
    private int _evtCount = 0;

    public ProductManagerActor(CatalogLoader catalogLoader)
    {
        CatalogLoader = catalogLoader;
        
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

        Command<ProductManagerMessages.Q.GetTopCategories>(cmd =>
        {
            Sender.Tell(CatalogLoader.TopCategories, Self);
        });
        Command<ProductManagerMessages.Q.GetCategoryProducts>(cmd =>
        {
            var r = CatalogLoader.GetCategoryProducts(cmd.Category, cmd.Paging);
            foreach (var p in r.Products)
            {
                if (!_productPrices.TryGetValue(p.Id, out var price))
                {
                    continue;
                }
                p.Price = price;
            }
            Sender.Tell(r, Self);
        });
        Command<ProductManagerMessages.Q.GetProductDetails>(cmd =>
        {
            var r = CatalogLoader.GetProductDetails(cmd.ProductId);
            if (_productPrices.TryGetValue(cmd.ProductId, out var price))
            {
                r.Price = price;
            }
            Sender.Tell(r, Self);
        });

        Command<ProductManagerMessages.C.UpdateProductPrice>(cmd =>
        {
            Logger.Info("ProductManagerActor: updating product {0} price to {1}.", cmd.ProductId, cmd.NewPrice);
            var e = new IntegrationMessages.E.ProductPriceUpdated(cmd.ProductId, cmd.NewPrice);
            Persist(e, evt =>
            {
                Apply(evt, false);
                MakeSnapshotIfNeeded();
            });
        });
        
        Command<SaveSnapshotSuccess>(_ =>
        {
            Logger.Info("Save snapshot success.");
        });
    }

    private void Apply(IntegrationMessages.E.ProductPriceUpdated evt, bool restoring)
    {
        _productPrices[evt.ProductId] = evt.NewPrice;
        
        if (!restoring)
        {
            Mediator.Tell(new Publish(Topics.ProductPriceUpdated(evt.ProductId), evt));
        }
    }

    public override string PersistenceId => "product-manager";
    
    public class State
    {
        public required ProductPrice[] ProductPrices { get; init; }

        public static object GetState(ProductManagerActor productManagerActor)
        {
            return new State
            {
                ProductPrices = productManagerActor
                    ._productPrices
                    .Select(kvp => new ProductPrice(kvp.Key, kvp.Value))
                    .ToArray()
            };
        }
        
        public void FillState(ProductManagerActor productManagerActor)
        {
            foreach (var (productId, price) in ProductPrices)
            {
                productManagerActor._productPrices[productId] = price;
            }
        }
        
        public record ProductPrice(int ProductId, decimal Price);
    }
    
    private void MakeSnapshotIfNeeded()
    {
        _evtCount++;
        if (_evtCount % 50 == 0)
        {
            SaveSnapshot(ProductManagerActor.State.GetState(this));
        }
    }
}