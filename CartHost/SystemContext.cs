using Akka.Actor;
using Akka.Hosting;
using CartHost.CartManager;
using SharedMessages;

namespace CartHost;

public static class SystemContext
{
    public static async Task<IActorRef> GetCart(this IRequiredActor<CartManagerActor> manager, int id)
    {
        return await manager.ActorRef.Ask<IActorRef>(new GetCart(id));
    }
}