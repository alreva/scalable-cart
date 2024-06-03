
using Orleans.Configuration;

namespace CartHost.Orleans.Client;

public static class OrleansService
{
    public static void ConfigureOrleansClient(this IServiceCollection services)
    {
        services
            .AddOrleansClient(client =>
            {
                client.UseLocalhostClustering();
            })
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "dev";
                options.ServiceId = "OrleansExample";
            })
            .AddSignalR();
    }
}