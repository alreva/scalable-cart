using CartHost.Orleans.Grains;
using CartHost.Orleans.Grains.CartNotifier;
using Microsoft.AspNetCore.SignalR;

namespace CartHost.Orleans.Cart;

public class CartHub(
    IGrainFactory grains
) : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        var cartId = Convert.ToInt32(Context.GetHttpContext()!.Request.Query["cartId"]);
        var cartNotifier = grains.GetGrain<ICartNotifierGrain>(WellKnownGrainIds.CartNotifier);
        await cartNotifier.RegisterCartConnected(Context.ConnectionId, cartId);
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        var cartNotifier = grains.GetGrain<ICartNotifierGrain>(WellKnownGrainIds.CartNotifier);
        await cartNotifier.RegisterCartDisconnected(Context.ConnectionId);
    }
}