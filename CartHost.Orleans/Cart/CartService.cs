using CartHost.Orleans.Grains.Cart;
using CartHost.Orleans.Grains.CartNotifier;
using Microsoft.AspNetCore.Mvc;

namespace CartHost.Orleans.Cart;

public static class CartService
{
    public static void MapCart(this WebApplication app)
    {
        app.MapGet("/cart/{id:int}", async (
                HttpContext ctx,
                int id,
                [FromServices] IGrainFactory grains) =>
            {
                var cart = grains.GetGrain<ICartGrain>(id);
                var cartDetails = await cart.GetDetails();
                return Results.Ok(new
                {
                    Path = $"/cart/{id}",
                    Details = cartDetails
                });
            })
            .WithName("GetCartById");

        app.MapPost("/cart/{id:int}/add-product", async (
                HttpContext ctx,
                int id,
                [FromBody] AddProductToCartRequest req,
                [FromServices] IGrainFactory grains) =>
            {
                var cart = grains.GetGrain<ICartGrain>(id);
                await cart.Add(new (req.ProductId, req.Price));
                return Results.Ok();
            })
            .WithName("AddProductToCart");
        app.MapHub<CartHub>("/hubs/cart");
    }

    public static void AddCart(this IServiceCollection services)
    {
        services.AddScoped<ICartNotifier, CartHubNotifier>();
    }
}