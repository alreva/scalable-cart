using Akka.Actor;
using Akka.Hosting;
using CartHost.Cart;
using CartHost.CartManager;
using SharedMessages;

namespace CartHost;

public static class SystemContext
{
    public static async Task<IActorRef> GetCart(this IRequiredActor<CartManagerActor> manager, int id)
    {
        return await manager.ActorRef.Ask<IActorRef>(new GetCart(id));
    }
    
    public static IActorRef GetCart(this ActorSystem system, int id)
    {
        return system.ActorOf(Props.Create(() => new CartActor(id)));
    }
}