using SharedMessages;

namespace CartHost.Orleans.Grains.ProductManager;

public interface IProductGrain : IGrainWithIntegerKey
{
    Task<ProductManagerMessages.CatalogItem> GetDetails();
    Task UpdatePrice(decimal price);
}