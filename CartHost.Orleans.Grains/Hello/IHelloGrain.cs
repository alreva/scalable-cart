using Orleans;

namespace CartHost.Orleans.Grains.Hello;

public interface IHelloGrain : IGrainWithGuidKey
{
    [Alias("SayHello")]
    Task<string> SayHello(string greeting);
}