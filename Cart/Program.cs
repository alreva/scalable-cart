// See https://aka.ms/new-console-template for more information

using Akka.Actor;
using Akka.Configuration;
using Cart;
using SharedMessages;
using Console = Cart.Console;

Console.WriteLine("Hello from Cart!");
Random r = new Random(123);

const string akkaPersistenceConnectionString = "Host=localhost;Port=5432;Database=akka-db;Username=akka;Password=1qaz@WSX;";

var config = 
#region Akka Configuration

    ConfigurationFactory.ParseString($$"""
       akka {
           actor.provider = cluster
           remote {
               dot-netty.tcp {
                   port = 8081
                   hostname = localhost
               }
           }
           cluster {
               seed-nodes = ["akka.tcp://ClusterSystem@localhost:8081"]
               pub-sub {
                   send-to-dead-letters-when-no-subscribers = off
               }
               roles = ["cart"]
           }
           persistence {
               journal {
                   plugin = "akka.persistence.journal.postgresql" # Identifies which journal plugin to use
                   postgresql {
                       class = "Akka.Persistence.PostgreSql.Journal.PostgreSqlJournal, Akka.Persistence.PostgreSql"
                       plugin-dispatcher = "akka.actor.default-dispatcher"
                       connection-string = "{{akkaPersistenceConnectionString}}" # Update with your credentials
                       schema-name = public
                       table-name = event_journal
                       auto-initialize = on
                       timestamp-provider = "Akka.Persistence.Sql.Common.Journal.DefaultTimestampProvider, Akka.Persistence.Sql.Common"
                   }
               }
               snapshot-store {
                   plugin = "akka.persistence.snapshot-store.postgresql" # Identifies which snapshot plugin to use
                   postgresql {
                       class = "Akka.Persistence.PostgreSql.Snapshot.PostgreSqlSnapshotStore, Akka.Persistence.PostgreSql"
                       plugin-dispatcher = "akka.actor.default-dispatcher"
                       connection-string = "{{akkaPersistenceConnectionString}}" # Update with your credentials
                       schema-name = public
                       table-name = snapshot_store
                       auto-initialize = on
                   }
               }
           }
           stdout-loglevel = ERROR
           loglevel = ERROR
           log-config-on-start = off
       }
       """);

#endregion

using (var system = ActorSystem.Create("ClusterSystem", config))
{
    Thread.Sleep(TimeSpan.FromSeconds(2));
    
    List<IActorRef> actors = new();
    
    foreach (var id in Enumerable.Range(1, 10))
    {
        var cart = system.ActorOf(
            Props.Create(() => new CartActor(id)),
            "cartActor-" + id);
        var response = await cart.Ask<HasProducts>(new HasProducts());
        switch (response)
        {
            case HasProducts.Yes _:
                Console.WriteLine($"Cart {id} has products");
                break;
            case HasProducts.No _:
                Console.WriteLine($"Cart {id} has no products. Adding...");
                cart.Tell(new AddProduct($"product{r.Next(1, 100)}", 10.0m));
                cart.Tell(new AddProduct($"product{r.Next(1, 100)}", 20.0m));
                break;
        }
        actors.Add(cart);
    }
    
    Console.WriteLineAnyways("Generated all the carts");

    var publisher = system.ActorOf<PriceUpdatePublisher>("publisher");
    actors.Add(publisher);
    
    Console.ReadLine();
}