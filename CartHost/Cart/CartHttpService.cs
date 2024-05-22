using Akka.Actor;
using Akka.Hosting;
using Microsoft.AspNetCore.Mvc;
using SharedMessages;

namespace CartHost.Cart;

public static class CartHttpService
{
    public static void MapCart(this WebApplication app)
    {
        app.MapGet("/cart/{id:int}", async (
                HttpContext ctx,
                int id,
                IRequiredActor<CartManagerActor> mgr) =>
            {
                var cart = await mgr.GetCart(id);
                var cartDetails = await cart.Ask<CartMessages.CartDetails>(CartMessages.Q.GetCartDetails.Instance);
                return Results.Ok(new
                {
                    Path = cart.Path.ToString(),
                    Details = cartDetails
                });
            })
            .WithName("GetCartById")
            .WithOpenApi();

        app.MapPost("/cart/{id:int}/add-product", async (
                HttpContext ctx,
                int id,
                [FromBody] AddProductToCartRequest req,
                [FromServices] IRequiredActor<CartManagerActor> mgr) =>
            {
                var cart = await mgr.GetCart(id);
                cart.Tell(new CartMessages.C.AddProduct(req.ProductName, req.Price));
                return Results.Ok();
            })
            .WithName("AddProductToCart")
            .WithOpenApi();
    }
}