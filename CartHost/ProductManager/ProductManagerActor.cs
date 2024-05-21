using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using SharedMessages;

namespace CartHost.ProductManager;

public class ProductManagerActor: ReceiveActor, ILogReceive
{
    private static readonly IActorRef Mediator = DistributedPubSub.Get(Context.System).Mediator;
    
    public ProductManagerActor()
    {
        Receive<UpdateProductPrice>(cmd =>
        {
            Mediator.Tell(new Publish(
                Topics.ProductPriceUpdated(cmd.ProductName),
                new IntegrationMessages.ProductPriceUpdated(cmd.ProductName, cmd.NewPrice)));
        });
    }
}