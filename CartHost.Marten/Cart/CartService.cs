using CartHost.Marten.Orchestration;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace CartHost.Marten.Cart;

public static class CartService
{
    public static void MapCart(this WebApplication app)
    {
        app.MapGet("/cart/{id}", async (
                HttpContext ctx,
                string id,
                [FromServices] IQuerySession querySession) =>
            {
                var cart = await querySession.Events.AggregateStreamAsync<Domain.Cart.Cart>(id);
                
                if (cart is null)
                {
                    return Results.Ok(new
                    {
                        Path = $"/cart/{id}",
                        Details = new Domain.Cart.Cart.M.CartDetails(id, [], 0)
                    });
                }
                
                var cartDetails = cart.GetDetails();
                return Results.Ok(new
                {
                    Path = $"/cart/{id}",
                    Details = cartDetails
                });
            })
            .WithName("GetCartById");

        app.MapPost("/cart/{id:int}/products", async (
                HttpContext ctx,
                int id,
                [FromBody] AddProductToCartRequest req,
                [FromServices] IDocumentStore store,
                [FromServices] IQuerySession query,
                [FromServices] ProductManager productManager) =>
            {
                await DocumentStoreExtensions.Upsert<Domain.Cart.Cart>(
                    store,
                    query,
                    id.ToString(),
                    new Domain.Cart.Cart.E.ProductAdded(req.ProductId, req.Price));
                await productManager.RegisterProductInCart((req.ProductId, id.ToString()));
                return Results.Ok();
            })
            .WithName("AddProductToCart");
        app.MapHub<CartHub>("/hubs/cart");
    }

    public static void AddCart(this WebApplicationBuilder builder)
    {
        builder.RequireProductManagement();
    }
}