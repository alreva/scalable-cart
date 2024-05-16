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

                    registry.Register<CartManagerActor>(system.ActorOf<CartManagerActor>());
                    registry.Register<ProductManagerActor>(system.ActorOf<ProductManagerActor>());
                });
        });
    }
}