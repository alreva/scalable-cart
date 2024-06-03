using CartHost.Orleans.Grains.CartNotifier;
using Microsoft.AspNetCore.SignalR;

namespace CartHost.Orleans.Cart;

internal class CartHubNotifier(
    IHubContext<CartHub> hubContext
) : ICartNotifier
{
    public Task SendNotification(string clientId, object message)
    {
        return hubContext.Clients.Client(clientId).SendAsync("ReceiveMessage", message);
    }
}