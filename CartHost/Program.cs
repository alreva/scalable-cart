using Akka.Actor;
using Akka.Actor.Dsl;
using Akka.Cluster.Hosting;
using Akka.Hosting;
using Akka.Persistence.PostgreSql.Hosting;
using CartHost;
using CartHost.CartManager;
using CartHost.ProductManager;
using Microsoft.AspNetCore.Mvc;
using SharedMessages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAkka("ActorSystem", akka =>
{
    akka
        .WithClustering()
        .WithPostgreSqlPersistence(
            builder.Configuration.GetConnectionString("AkkaPersistence")!,
            autoInitialize: true)
        .WithActors((system, registry) =>
        {
            var echo = system.ActorOf(dsl =>
            {
                dsl.ReceiveAny((o, context) =>
                {
                    context.Sender.Tell("Echo: " + o);
                });
            });
            registry.TryRegister<CartManagerActor>(system.ActorOf<CartManagerActor>(), true);
            registry.TryRegister<ProductManagerActor>(system.ActorOf<ProductManagerActor>(), true);
            registry.Register<Echo>(echo);
        });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", async (HttpContext ctx, IRequiredActor<Echo> echo) =>
{
    var echoMessage = await echo.ActorRef.Ask(ctx.TraceIdentifier, ctx.RequestAborted);
    return Results.Ok(echoMessage);
});

app.MapGet("/cart/{id:int}", async (HttpContext ctx, int id, IRequiredActor<CartManagerActor> mgr) =>
    {
        var cart = await mgr.GetCart(id);
        var cartDetails = await cart.Ask<CartDetails>(GetCartDetails.Instance);
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
        cart.Tell(new AddProduct(req.ProductName, req.Price));
        return Results.Ok();
    })
    .WithName("AddProductToCart")
    .WithOpenApi();

app.MapPut("/product/{productName}", async (
        HttpContext ctx,
        string productName,
        [FromBody] ProductUpdateRequest req,
        [FromServices] IRequiredActor<ProductManagerActor> mgr) =>
    {
        mgr.ActorRef.Tell(new UpdateProductPrice(productName, req.Price));
    })
    .WithName("UpdateProduct")
    .WithOpenApi();

app.Run();

class Echo
{
    
}

public static class SystemContext
{
    public static async Task<IActorRef> GetCart(this IRequiredActor<CartManagerActor> manager, int id)
    {
        return await manager.ActorRef.Ask<IActorRef>(new GetCart(id));
    }
}

public record AddProductToCartRequest(string ProductName, decimal Price);

public record ProductUpdateRequest(decimal Price);