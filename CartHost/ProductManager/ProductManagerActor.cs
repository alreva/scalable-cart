using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using CartHost.ProductManager.Catalog;
using SharedMessages;

namespace CartHost.ProductManager;

public class ProductManagerActor: ReceiveActor, ILogReceive
{ 
    public CatalogLoader CatalogLoader { get; }
    private static readonly IActorRef Mediator = DistributedPubSub.Get(Context.System).Mediator;
    
    public ProductManagerActor(CatalogLoader catalogLoader)
    {
        CatalogLoader = catalogLoader;

        Receive<ProductManagerMessages.Q.GetTopCategories>(cmd =>
        {
            Sender.Tell(CatalogLoader.TopCategories, Self);
        });
        Receive<ProductManagerMessages.Q.GetCategoryProducts>(cmd =>
        {
            Sender.Tell(CatalogLoader.GetCategoryProducts(cmd.Category, cmd.Paging), Self);
        });

        Receive<ProductManagerMessages.C.UpdateProductPrice>(cmd =>
        {
            Mediator.Tell(new Publish(
                Topics.ProductPriceUpdated(cmd.ProductName),
                new IntegrationMessages.E.ProductPriceUpdated(cmd.ProductName, cmd.NewPrice)));
        });
    }
}