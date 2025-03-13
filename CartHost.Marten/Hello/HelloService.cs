using Marten;

namespace CartHost.Marten.Hello;

public static class HelloService
{
    public static void MapHello(this WebApplication app)
    {
        app.MapGet("/hello", static async (IQuerySession querySession) =>
        {
            var hello = await querySession.Events.AggregateStreamAsync<Domain.Hello.Hello>("Hello, World!");
            return hello is not null ? hello.Id : "Hello, World!";
        });
    }
}