using Akka.Actor;
using Akka.Hosting;
using Microsoft.AspNetCore.SignalR;
using SharedMessages;

namespace CartHost.Cart;

public class CartHub(IRequiredActor<CartChangesNotificationActor> mgr) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var cartId = Convert.ToInt32(Context.GetHttpContext()!.Request.Query["cartId"]);
        mgr.ActorRef.Tell(new CartChangesMessages.ClientCartConnected(Context.ConnectionId, cartId), ActorRefs.Nobody);
        
        await Clients.Clients([Context.ConnectionId]).SendAsync("ReceiveMessage", new { Message="Hello from hub!" });
        
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        mgr.ActorRef.Tell(new CartChangesMessages.ClientCartDisconnected(Context.ConnectionId), ActorRefs.Nobody);
        return base.OnDisconnectedAsync(exception);
    }
}

public static class HubContext
{
    public static IHubContext<CartHub> Cart { get; set; }
}