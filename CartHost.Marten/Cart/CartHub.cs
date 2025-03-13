using CartHost.Marten.Orchestration;
using Microsoft.AspNetCore.SignalR;

namespace CartHost.Marten.Cart;

public class CartHub(
    CartRegistry cartRegistry
) : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        string cartId = Context.GetHttpContext()!.Request.Query["cartId"]!;
        await cartRegistry.RegisterCartConnected((cartId,Context.ConnectionId));
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        await cartRegistry.RegisterCartDisconnected(Context.ConnectionId);
    }
}