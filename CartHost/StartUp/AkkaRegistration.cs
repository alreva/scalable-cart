using Akka.Actor;
using Akka.Actor.Dsl;
using Akka.Cluster.Hosting;
using Akka.Hosting;
using Akka.Persistence.PostgreSql.Hosting;
using CartHost.CartManager;
using CartHost.ProductManager;

namespace CartHost.StartUp;

public static class AkkaRegistration
{
    public static void ConfigureAkka(this IServiceCollection services, string akkaPersistencePgConnectionString)
    {
        services.AddAkka("ActorSystem", akka =>
        {
            akka
                .WithClustering()
                .WithPostgreSqlPersistence(
                    akkaPersistencePgConnectionString,
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
                    registry.Register<IEcho>(echo);
                });
        });
    }
}