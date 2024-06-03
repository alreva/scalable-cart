using CartHost.Orleans.Grains.CartNotifier;
using CartHost.Orleans.Grains.ProductManager;
using Microsoft.Extensions.Logging;
using Orleans.Providers;
using Orleans.Runtime;
using SharedMessages;

namespace CartHost.Orleans.Grains.Cart;

internal class CartGrain(
    ILogger<CartGrain> log,
    IGrainFactory grains,
    [PersistentState("cart")] IPersistentState<CartGrainState> state
) : Grain<CartGrainState>(state), ICartGrain
{
    public Task<CartMessages.CartDetails> GetDetails()
    {
        return Task.FromResult(BuildCartDetails());
    }

    public async Task Add(CartMessages.C.AddProduct cmd)
    {
        log.LogInformation("Adding product {ProductId} to cart {CartId}", cmd.Id, this.GetPrimaryKeyLong());
        var existingItem = State.LineItems.Find(x => x.ProductId == cmd.Id);
        if (existingItem != null)
        {
            existingItem.Quantity += 1;
        }
        else
        {
            State.LineItems.Add(new(cmd.Id, cmd.Price, 1));
        }
        RecalculateTotalPrice();
        await WriteStateAsync();
        
        var productManager = grains
            .GetGrain<IProductManagerGrain>(WellKnownGrainIds.ProductManager);
        await productManager
            .SubscribeCartToProductUpdates((int)this.GetPrimaryKeyLong(), cmd.Id);
        await NotifyCartChanged();
    }

    private async Task NotifyCartChanged()
    {
        var notifier = grains.GetGrain<ICartNotifierGrain>(WellKnownGrainIds.CartNotifier);
        var id = this.GetPrimaryKeyLong();
        await notifier.NotifyCartChanged(new((int)id, BuildCartDetails()));
    }

    public async Task UpdateProductPrice(int productId, decimal price)
    {
        var lineItem = State.LineItems.Find(x => x.ProductId == productId);
        if (lineItem == null)
        {
            return;
        }
        lineItem.Price = price;
        RecalculateTotalPrice();
        await WriteStateAsync();
        await NotifyCartChanged();
    }

    private CartMessages.CartDetails BuildCartDetails()
    {
        return new(
            (int)this.GetPrimaryKeyLong(),
            State.LineItems.ToArray(),
            State.TotalPrice
        );
    }

    private void RecalculateTotalPrice()
    {
        State = State with
        {
            TotalPrice = State.LineItems.Sum(x => x.Price * x.Quantity)
        };
    }
}