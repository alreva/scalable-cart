using System.Net;
using CartHost.Marten.Domain;
using CartHost.Marten.Domain.Catalog;
using CartHost.Marten.Orchestration;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace CartHost.Marten.Catalog;

public static class CatalogService
{
    public static void AddProducts(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<CatalogPathProvider>();
        builder.Services.AddSingleton<ICatalog, Catalog>();
        builder.RequireProductManagement();
    }
    
    public static void MapProducts(this WebApplication app)
    {
        // warmup:
        app.Services.GetRequiredService<ICatalog>();
        
        app.MapGet(
            "/catalog/top-categories", (
                HttpContext ctx,
                [FromServices] ICatalog catalog
            ) =>
            {
                var topCategories = catalog.TopCategories;
                return Task.FromResult(Results.Ok(topCategories));
            });
        app.MapGet(
            "/catalog/category/{id}/products", (
                HttpContext ctx,
                [FromServices] ILogger<Program> log,
                string id,
                [FromServices] ICatalog catalogLoader,
                [FromQuery] int skip = 0,
                [FromQuery] int take = 10
            ) =>
            {
                log.LogInformation("Getting products for category {Id}", id);
                var products = catalogLoader
                    .GetCategoryProducts(
                        new Domain.Catalog.Catalog.Category(WebUtility.UrlDecode(id)),
                        new Paging(skip, take)
                    );
                return Task.FromResult(Results.Ok(products));
            });

        app.MapGet("/product/{id:int}", async (
                HttpContext ctx,
                int id,
                [FromServices] ICatalog catalogLoader
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
                [FromServices] ProductManager productManager) =>
            {
                await productManager.UpdateProductPrice((id, req.Price));
                return Results.NoContent();
            })
            .WithName("UpdateProduct");
    }
}