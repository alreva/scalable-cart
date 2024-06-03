using SharedMessages;

namespace CartHost.Orleans.Grains;

public interface ICatalogLoader
{
    ProductManagerMessages.TopCategories TopCategories { get; }

    ProductManagerMessages.CategoryProducts GetCategoryProducts(
        ProductManagerMessages.Category category,
        Paging paging = default);

    ProductManagerMessages.CatalogItem GetProductDetails(int id);
}