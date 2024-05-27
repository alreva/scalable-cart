using System.Net;
using Akka.Actor;
using Akka.Hosting;
using Microsoft.AspNetCore.Mvc;
using SharedMessages;

namespace CartHost.ProductManager;

public static class ProductHttpService
{
    public static void MapProducts(this WebApplication app)
    {
        app.MapGet(
            "/catalog/top-categories",
            async ([FromServices] IRequiredActor<ProductManagerActor> mgr) =>
            {
                var topCategories = await mgr.ActorRef.Ask<ProductManagerMessages.TopCategories>(
                    ProductManagerMessages.Q.GetTopCategories.Query);
                return Results.Ok(topCategories);
            });
        app.MapGet(
            "/catalog/category/{id}/products",
            async (
                HttpContext ctx,
                string id,
                [FromServices] IRequiredActor<ProductManagerActor> mgr,
                [FromQuery] int skip = 0,
                [FromQuery] int take = 10
            ) =>
            {
                    var query = ProductManagerMessages.Q.GetCategoryProducts
                    .Query(
                        new ProductManagerMessages.Category(WebUtility.UrlDecode(id)),
                        new Paging(skip, take)
                    );
                var products = await mgr.ActorRef.Ask<ProductManagerMessages.CategoryProducts>(query);
                return Results.Ok(products);
            });
        
        app.MapPut("/product/{productName}", async (
                HttpContext ctx,
                string productName,
                [FromBody] ProductUpdateRequest req,
                [FromServices] IRequiredActor<ProductManagerActor> mgr) =>
            {
                mgr.ActorRef.Tell(new ProductManagerMessages.C.UpdateProductPrice(productName, req.Price));
            })
            .WithName("UpdateProduct")
            .WithOpenApi();
    }
}