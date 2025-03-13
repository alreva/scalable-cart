using System.Collections.Concurrent;
using System.Text.Json;
using CartHost.Marten.Domain;
using CartHost.Marten.Domain.Catalog;

namespace CartHost.Marten.Catalog;

public class Catalog : ICatalog
{
    private readonly Domain.Catalog.Catalog.CatalogItem[] _catalog;

    private readonly ConcurrentDictionary<
        Domain.Catalog.Catalog.Category,
        CategoryProducts
    > _categoryProductsCache = new();

    private readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public Domain.Catalog.Catalog.TopCategories TopCategories { get; }

    public Catalog(
        CatalogPathProvider catalogPathProvider,
        ILogger<Catalog> log
    )
    {
        try
        {
            var jsonStream = File.OpenRead(catalogPathProvider.CatalogPath);
            _catalog = JsonSerializer.Deserialize<Domain.Catalog.Catalog.CatalogItem[]>(jsonStream, _options)!;
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

    private Domain.Catalog.Catalog.TopCategories GetTopCategories()
    {
        // Extract all unique categories
        var categories = _catalog
            .Select(item => new Domain.Catalog.Catalog.Category(item.Category))
            .OrderBy(category => category.Name)
            .Distinct()
            .ToArray();

        return new Domain.Catalog.Catalog.TopCategories(categories);
    }

    public Domain.Catalog.Catalog.CategoryProducts GetCategoryProducts(
        Domain.Catalog.Catalog.Category category,
        Paging paging = default)
    {
        var allCategoryProducts = _categoryProductsCache.GetOrAdd(category, GetCategoryProductsFromCatalog(category));

        var productsPage = allCategoryProducts
            .Products
            .Skip(paging.Skip)
            .Take(paging.Take)
            .ToArray();
        return new(category, productsPage, allCategoryProducts.TotalProducts);
    }

    private CategoryProducts GetCategoryProductsFromCatalog(Domain.Catalog.Catalog.Category category)
    {
        var allProducts = _catalog
            .Where(item => item.Category == category.Name)
            .OrderBy(item => item.Name);
        var totalProducts = allProducts.Count();
        return new CategoryProducts(allProducts, totalProducts);
    }

    public Domain.Catalog.Catalog.CatalogItem GetProductDetails(int id)
    {
        return _catalog.First(item => item.Id == id);
    }

    private sealed record CategoryProducts(IEnumerable<Domain.Catalog.Catalog.CatalogItem> Products, int TotalProducts);
}

public class CatalogPathProvider(IConfiguration config)
{
    public string CatalogPath { get; set; } = config.GetConnectionString("CatalogManager")!;
}