using CartHost.Marten.Cart;
using Microsoft.AspNetCore.SignalR;

namespace CartHost.Marten.Orchestration;

public class CartHubNotifier(
    IHubContext<CartHub> hubContext
)
{
    public Task SendNotification(string clientId, Domain.Cart.Cart.M.CartDetails cartDetails)
    {
        return hubContext.Clients.Client(clientId).SendAsync("ReceiveMessage", cartDetails);
    }
}