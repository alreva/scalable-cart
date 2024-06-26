using System.Net;
using CartHost.Orleans.Grains;
using CartHost.Orleans.Grains.ProductManager;
using CartHost.Orleans.ProductManager.Catalog;
using Microsoft.AspNetCore.Mvc;
using SharedMessages;

namespace CartHost.Orleans.ProductManager;

public static class ProductService
{
    public static void AddProducts(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<CatalogPathProvider>();
        builder.Services.AddSingleton<ICatalogLoader, CatalogLoader>();
    }
    
    public static void MapProducts(this WebApplication app)
    {
        // warmup:
        app.Services.GetRequiredService<ICatalogLoader>();
        
        app.MapGet(
            "/catalog/top-categories", (
                HttpContext ctx,
                [FromServices] ICatalogLoader catalogLoader
            ) =>
            {
                var topCategories = catalogLoader.TopCategories;
                return Task.FromResult(Results.Ok(topCategories));
            });
        app.MapGet(
            "/catalog/category/{id}/products", (
                HttpContext ctx,
                [FromServices] ILogger<Program> log,
                string id,
                [FromServices] ICatalogLoader catalogLoader,
                [FromQuery] int skip = 0,
                [FromQuery] int take = 10
            ) =>
            {
                log.LogInformation("Getting products for category {id}", id);
                var products = catalogLoader
                    .GetCategoryProducts(
                        new ProductManagerMessages.Category(WebUtility.UrlDecode(id)),
                        new Paging(skip, take)
                    );
                return Task.FromResult(Results.Ok(products));
            });

        app.MapGet("/product/{id:int}", async (
                HttpContext ctx,
                int id,
                [FromServices] ICatalogLoader catalogLoader
            ) =>
            {
                var product = catalogLoader.GetProductDetails(id);
                return Results.Ok(product);
            })
            .WithName("GetProduct");
        
        app.MapPut("/product/{id:int}", async (
                HttpContext ctx,
                int id,
                [FromBody] ProductUpdateRequest req,
                [FromServices] IGrainFactory grains) =>
            {
                var productManager = grains.GetGrain<IProductManagerGrain>(WellKnownGrainIds.ProductManager);
                await productManager.UpdateProductPrice(id, req.Price);
                return Results.NoContent();
            })
            .WithName("UpdateProduct");
    }
}