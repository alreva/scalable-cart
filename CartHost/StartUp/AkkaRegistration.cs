using Akka.Actor;
using Akka.Actor.Dsl;
using Akka.Cluster.Hosting;
using Akka.Hosting;
using Akka.Persistence.PostgreSql.Hosting;
using CartHost.CartManager;
using CartHost.ProductManager;
using LogLevel = Akka.Event.LogLevel;

namespace CartHost.StartUp;

public static class AkkaRegistration
{
    public static void ConfigureAkka(this IServiceCollection services, IConfiguration config)
    {
        var cartHostConfig = new CartHostConfiguration();
        config.Bind("CartHost", cartHostConfig);
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

                    registry.Register<CartManagerActor>(system.ActorOf<CartManagerActor>("cart-manager"));
                    registry.Register<ProductManagerActor>(system.ActorOf<ProductManagerActor>("product-manager"));
                });
        });
    }
}

public class CartHostConfiguration
{
    public bool LogAkkaDebugMessages { get; set; }
}