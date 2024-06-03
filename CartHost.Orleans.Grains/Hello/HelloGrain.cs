using Microsoft.Extensions.Logging;
using Orleans;

namespace CartHost.Orleans.Grains.Hello;

internal class HelloGrain(ILogger<HelloGrain> log) : Grain, IHelloGrain
{
    public Task<string> SayHello(string greeting)
    {
        log.LogInformation("SayHello called with greeting: {greeting}", greeting);
        return Task.FromResult($"You said: '{greeting}', I say: Hello!");
    }
}