using Akka.Actor;
using Akka.Cluster.Tools.Client;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Cluster.Tools.Singleton;
using Akka.Configuration;
using Cocona;
using Microsoft.Extensions.DependencyInjection;
using SharedMessages;

Console.WriteLine("Hello, this is the CLI for your domain commands speaking!");
var builder = CoconaApp.CreateBuilder();
var app = builder.Build();

app.AddCommand(
    "cart", 
    async (string cartId) =>
{
    using var system = CreateActorSystem();
    Console.WriteLine($"Managing cart {cartId}");
    var cart = system.ActorSelection("akka.tcp://ClusterSystem@localhost:8081/user/cartActor-" + cartId);
    var cartDetails = await cart.Ask<CartDetails>(GetCartDetails.Instance);
    Console.WriteLine("Cart details:");
    Console.WriteLine($"Cart ID: {cartDetails.CartId}");
    Console.WriteLine($"Total price: {cartDetails.TotalPrice}");
    Console.WriteLine("Line items:");
    foreach (var item in cartDetails.LineItems)
    {
        Console.WriteLine($"- {item.ProductName} ({item.Price} x {item.Quantity} = {item.Price * item.Quantity})");
    }
});

app.AddCommand(
    "product-price",
    async (string productName, decimal price) =>
    {
        using var system = CreateActorSystem();
        Console.WriteLine($"Updating price of product {productName} to {price}");
        var publisher = system.ActorSelection("akka.tcp://ClusterSystem@localhost:8081/user/publisher");
        publisher.Tell(new ProductPriceUpdated(productName, price));
    }
);

await app.RunAsync();

static ActorSystem CreateActorSystem()
{
    var config = 
        #region Akka Configuration

        ConfigurationFactory.ParseString($$"""
                                           akka {
                                               actor.provider = cluster
                                               remote {
                                                   dot-netty.tcp {
                                                       port = 0
                                                       hostname = localhost
                                                   }
                                               }
                                               cluster {
                                                   seed-nodes = ["akka.tcp://ClusterSystem@localhost:8081"]
                                                   pub-sub {
                                                       send-to-dead-letters-when-no-subscribers = off
                                                   }
                                                   roles = ["cli"]
                                               }
                                               stdout-loglevel = ERROR
                                               loglevel = ERROR
                                               log-config-on-start = off
                                           }
                                           """);

    config = config
        .WithFallback(ClusterSingletonManager.DefaultConfig())
        .WithFallback(DistributedPubSub.DefaultConfig())
        .WithFallback(ClusterClientReceptionist.DefaultConfig());

    #endregion

    var actorSystem = ActorSystem.Create("ClusterSystem", config);
    Thread.Sleep(TimeSpan.FromSeconds(2));
    return actorSystem;
}