using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using SharedMessages;

namespace Cart;

public class PriceUpdatePublisher: ReceiveActor
{
    private readonly IActorRef _mediator = DistributedPubSub.Get(Context.System).Mediator;

    public PriceUpdatePublisher()
    {
        Receive<ProductPriceUpdated>(update =>
        {
            Console.WriteLine($"Publishing update for {update.ProductName} with new price {update.NewPrice}");
            _mediator.Tell(new Publish(Topics.ProductPriceUpdated(update.ProductName), update));
        });
    }
}