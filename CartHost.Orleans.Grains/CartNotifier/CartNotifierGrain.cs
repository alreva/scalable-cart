using Microsoft.Extensions.Logging;
using SharedMessages;

namespace CartHost.Orleans.Grains.CartNotifier;

public interface ICartNotifierGrain : IGrainWithStringKey
{
    [Alias("NotifyCartChanged")]
    Task NotifyCartChanged(IntegrationMessages.E.CartChanged evt);

    [Alias("RegisterCartConnected")]
    Task RegisterCartConnected(string clientId, int cartId);
    
    [Alias("RegisterCartDisconnected")]
    Task RegisterCartDisconnected(string clientId);
}

internal class CartNotifierGrain(
    ILogger<CartNotifierGrain> log,
    ICartNotifier notifier
    ) : Grain<CartNotifierGrainState>, ICartNotifierGrain
{
    private List<(int CartId, string ClientId)> Carts => State.Carts;

    public async Task NotifyCartChanged(IntegrationMessages.E.CartChanged evt)
    {
        log.LogInformation("Notifying all clients that cart {CartId} changed", evt.CartId);
        log.LogTrace("Carts: {Carts}", Carts);
        var clientIds = Carts
            .Where(x => x.CartId == evt.CartId)
            .Select(x => x.ClientId).ToArray();
        if (clientIds.Length == 0)
        {
            return;
        }

        await Task.WhenAll(
            clientIds
                .Select(clientId =>
                {
                    log.LogInformation(
                        "Notifying client {clientId} of cart {CartId} change",
                        clientId,
                        evt.CartId
                    );
                    return notifier.SendNotification(clientId, evt.Details);
                }));
    }

    public async Task RegisterCartConnected(string clientId, int cartId)
    {
        log.LogInformation("Registering cart {cartId} for client {clientId}", cartId, clientId);
        Carts.Add((cartId, clientId));
        await WriteStateAsync();
    }

    public async Task RegisterCartDisconnected(string clientId)
    {
        log.LogInformation("Unregistering client {clientId}", clientId);
        Carts.RemoveAll(x => x.ClientId == clientId);
        await WriteStateAsync();
    }
}

public class CartNotifierGrainState()
{
    public List<(int CartId, string ClientId)> Carts { get; init; } = [];

    public void Deconstruct(out List<(int CartId, string ClientId)> carts)
    {
        carts = this.Carts;
    }
}

public interface ICartNotifier
{
    Task SendNotification(string clientId, object message);
}