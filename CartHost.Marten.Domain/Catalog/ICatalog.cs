namespace CartHost.Marten.Domain.Catalog;

public interface ICatalog
{
    Catalog.TopCategories TopCategories { get; }

    Catalog.CategoryProducts GetCategoryProducts(
        Catalog.Category category,
        Paging paging = default);

    Catalog.CatalogItem GetProductDetails(int id);
}