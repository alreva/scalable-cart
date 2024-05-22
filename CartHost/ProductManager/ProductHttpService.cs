using Akka.Actor;
using Akka.Hosting;
using Microsoft.AspNetCore.Mvc;
using SharedMessages;

namespace CartHost.ProductManager;

public static class ProductHttpService
{
    public static void MapProducts(this WebApplication app)
    {
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