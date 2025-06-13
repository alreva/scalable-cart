using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SharedMessages;

namespace CatalogManager;

public class CatalogApiClient(HttpClient http, ILogger<CatalogApiClient> log)
{
    public async Task<ProductManagerMessages.TopCategories> GetTopCategories()
    {
        log.LogInformation("Fetching top categories from catalog service");
        var r = await http
            .GetFromJsonAsync(
                "catalog/top-categories",
                CatalogManagerJsonSerialization.Default.TopCategories
            );
            
        return r!;
    }
    
    public async Task<ProductManagerMessages.CategoryProducts> GetCategoryProducts(string categoryName)
    {
        log.LogInformation("Fetching products for category: {CategoryName}", categoryName);
        var r = await http.GetFromJsonAsync(
                $"catalog/category/{categoryName}/products",
                CatalogManagerJsonSerialization.Default.CategoryProducts
            );
        return r!;
    }

    public async Task<ProductManagerMessages.CatalogItem?> GetProductDetails(int id)
    {
        log.LogInformation("Fetching product details for ID: {ProductId}", id);
        var r = await http.GetFromJsonAsync(
            $"product/{id}",
            CatalogManagerJsonSerialization.Default.CatalogItem);
        return r;
    }

    public async Task UpdateProductPrice(int id, decimal price)
    {
        log.LogInformation("Updating product price for ID: {ProductId} to {Price}", id, price);
        await http.PutAsJsonAsync(
            $"product/{id}",
            new { Price = price },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );
    }
}