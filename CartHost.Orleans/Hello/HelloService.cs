using CartHost.Orleans.Grains.Hello;

namespace CartHost.Orleans.Hello;

public static class HelloService
{
    public static void MapHello(this WebApplication app)
    {
        app.MapGet("/hello", static async (IClusterClient client, HttpRequest request) =>
        {
            var grain = client.GetGrain<IHelloGrain>(Guid.NewGuid());
            var response = await grain.SayHello("Hello, World!");
            return Results.Ok(response);
        });
    }
}