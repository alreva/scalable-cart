using System.Collections.Concurrent;
using System.Text.Json;
using CartHost.Orleans.Grains;
using SharedMessages;

namespace CartHost.Orleans.ProductManager.Catalog;

public class CatalogLoader : ICatalogLoader
{
    private readonly ProductManagerMessages.CatalogItem[] _catalog;

    private readonly ConcurrentDictionary<
        ProductManagerMessages.Category,
        CategoryProducts
    > _categoryProductsCache = new();

    private readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ProductManagerMessages.TopCategories TopCategories { get; }

    public CatalogLoader(
        CatalogPathProvider catalogPathProvider,
        ILogger<CatalogLoader> log
    )
    {
        try
        {
            var jsonStream = File.OpenRead(catalogPathProvider.CatalogPath);
            _catalog = JsonSerializer.Deserialize<ProductManagerMessages.CatalogItem[]>(jsonStream, _options)!;
            TopCategories = GetTopCategories();
        }
        catch (FileNotFoundException e)
        {
            var absolutePath = Path.GetFullPath(catalogPathProvider.CatalogPath);
            var exceptionMessage
                = $"""
                   Could not find catalog file at {absolutePath}.
                   Please check that the file exists.
                   If it does not, please download if from OneDrive here:
                   https://1drv.ms/u/s!AvI-lJAeKrg-gZ0mD-LkhjDPUXatjA?e=QTbs5W
                   Then unzip it and put in onto the path specified in the configuration.
                   """;
            log.LogError(
                e,
                """
                Could not find catalog file at {CatalogPath}.
                Please check that the file exists.
                If it does not, please download if from OneDrive here:
                https://1drv.ms/u/s!AvI-lJAeKrg-gZ0mD-LkhjDPUXatjA?e=QTbs5W
                Then unzip it and put in onto the path specified in the configuration.
                """,
                absolutePath);
            throw new FileNotFoundException(exceptionMessage, e);
        }
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
        var allCategoryProducts = _categoryProductsCache.GetOrAdd(category, GetCategoryProductsFromCatlog(category));

        var productsPage = allCategoryProducts
            .Products
            .Skip(paging.Skip)
            .Take(paging.Take)
            .ToArray();
        return new(category, productsPage, allCategoryProducts.TotalProducts);
    }

    private CategoryProducts GetCategoryProductsFromCatlog(ProductManagerMessages.Category category)
    {
        var allProducts = _catalog
            .Where(item => item.Category == category.Name)
            .OrderBy(item => item.Name);
        var totalProducts = allProducts.Count();
        return new CategoryProducts(allProducts, totalProducts);
    }

    public ProductManagerMessages.CatalogItem GetProductDetails(int id)
    {
        return _catalog.First(item => item.Id == id);
    }

    private record CategoryProducts(IEnumerable<ProductManagerMessages.CatalogItem> Products, int TotalProducts);
}

public class CatalogPathProvider(IConfiguration config)
{
    public string CatalogPath { get; set; } = config.GetConnectionString("CatalogManager")!;
}