using Akka.Actor;
using Akka.Actor.Dsl;
using Akka.Cluster.Hosting;
using Akka.Hosting;
using Akka.Persistence.PostgreSql.Hosting;
using CartHost.Cart;
using CartHost.ProductManager;
using CartHost.ProductManager.Catalog;
using Microsoft.AspNetCore.SignalR;
using LogLevel = Akka.Event.LogLevel;

namespace CartHost.StartUp;

public static class AkkaRegistration
{
    public static void ConfigureAkka(this IServiceCollection services, IConfiguration config)
    {
        var cartHostConfig = new CartHostConfiguration();
        config.Bind("CartHost", cartHostConfig);
        var productConfig = new ProductManagerConfiguration();
        config.Bind("ProductManager", productConfig);
        var logDebug = cartHostConfig.LogAkkaDebugMessages;
        services.AddAkka("ActorSystem", akka =>
        {
            akka
                .ConfigureLoggers(setup =>
                {
                    setup.DebugOptions = new DebugOptions
                    {
                        LifeCycle = logDebug,
                        AutoReceive = logDebug,
                        EventStream = logDebug,
                        FiniteStateMachine = logDebug,
                        Receive = logDebug,
                        RouterMisconfiguration = logDebug,
                        Unhandled = logDebug,
                    };
                    if (logDebug)
                    {
                        setup.LogLevel = LogLevel.DebugLevel;
                        setup.LogConfigOnStart = true;
                    }
                    setup.ClearLoggers();
                    setup.AddLoggerFactory();
                })
                .WithClustering()
                .WithPostgreSqlPersistence(
                    config.GetConnectionString("AkkaPersistence")!,
                    autoInitialize: true)
                .WithActors((system, registry) =>
                {
                    #region Demo Actor DSL

                    var echo = system.ActorOf(dsl =>
                    {
                        dsl.ReceiveAny((o, context) =>
                        {
                            context.Sender.Tell("Echo: " + o);
                        });
                    });
                    registry.Register<IEcho>(echo);

                    #endregion

                    registry.Register<CartChangesNotificationActor>(system.ActorOf<CartChangesNotificationActor>());
                    registry.Register<CartManagerActor>(system.ActorOf<CartManagerActor>("cart-manager"));
                    var catalogLoader = new CatalogLoader(productConfig.CatalogJsonPath);
                    var productManagerActorProps = Props.Create(() => new ProductManagerActor(catalogLoader));
                    registry.Register<ProductManagerActor>(system.ActorOf(productManagerActorProps, "product-manager"));
                });
        });
    }
}