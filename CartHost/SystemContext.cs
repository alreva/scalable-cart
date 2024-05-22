using Akka.Actor;
using Akka.Hosting;
using CartHost.Cart;
using SharedMessages;

namespace CartHost;

public static class SystemContext
{
    public static async Task<IActorRef> GetCart(this IRequiredActor<CartManagerActor> manager, int id)
    {
        return await manager.ActorRef.Ask<IActorRef>(new CartManagerMessages.GetCart(id));
    }
}