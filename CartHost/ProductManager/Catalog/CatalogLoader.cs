using System.Collections.Concurrent;
using System.Text.Json;
using SharedMessages;

namespace CartHost.ProductManager.Catalog;

public class CatalogLoader
{
    private readonly ProductManagerMessages.CatalogItem[] _catalog;

    private readonly ConcurrentDictionary<
        ProductManagerMessages.Category,
        IEnumerable<ProductManagerMessages.CatalogItem>
    > _categoryProductsCache = new();
    
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ProductManagerMessages.TopCategories TopCategories { get; }

    public CatalogLoader(string jsonFilePath)
    {
        var jsonStream = File.OpenRead(jsonFilePath);

        // Deserialize the JSON data into a list of CatalogItem objects
        _catalog = JsonSerializer.Deserialize<ProductManagerMessages.CatalogItem[]>(jsonStream, _options)!;
        TopCategories = GetTopCategories();
    }
    
    private ProductManagerMessages.TopCategories GetTopCategories()
    {
        // Extract all unique categories
        var categories = _catalog
            .Select(item => new ProductManagerMessages.Category(item.Category))
            .OrderBy(category => category.Name)
            .Distinct()
            .ToArray();

        return new ProductManagerMessages.TopCategories(categories);
    }
    
    public ProductManagerMessages.CategoryProducts GetCategoryProducts(
        ProductManagerMessages.Category category,
        Paging paging = default)
    {
        var allCategoryProducts = _categoryProductsCache.GetOrAdd(category, _catalog
            .Where(item => item.Category == category.Name)
            .OrderBy(item => item.Name));
        
        var productsPage = allCategoryProducts
            .Skip(paging.Skip)
            .Take(paging.Take)
            .ToArray();
        return new(category, productsPage);
    }
    
    public ProductManagerMessages.CatalogItem GetProductDetails(int id)
    {
        return _catalog.First(item => item.Id == id);
    }
}