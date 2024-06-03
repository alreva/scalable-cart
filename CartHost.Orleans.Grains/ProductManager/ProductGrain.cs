using Orleans.Providers;
using Orleans.Runtime;
using SharedMessages;

namespace CartHost.Orleans.Grains.ProductManager;

public class ProductGrain(
    ICatalogLoader catalog,
    [PersistentState("product")] IPersistentState<ProductGrainState> state
    ) : Grain<ProductGrainState>(state), IProductGrain
{
    public Task<ProductManagerMessages.CatalogItem> GetDetails()
    {
        var id = (int)this.GetPrimaryKeyLong();
        var product = catalog.GetProductDetails(id);
        if (State.Price is not null)
        {
            product.Price = State.Price.Value;
        }
        return Task.FromResult(product);
    }

    public async Task UpdatePrice(decimal price)
    {
        State = State with { Price = price };
        await WriteStateAsync();
    }
}