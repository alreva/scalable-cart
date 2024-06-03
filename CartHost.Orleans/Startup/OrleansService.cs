using Orleans.Configuration;

namespace CartHost.Orleans.Startup;

public static class OrleansService
{
    public static void ConfigureOrleansServer(this WebApplicationBuilder builder)
    {
        var adoNetInvariant = "Npgsql";
        var adoNetConnectionString = builder.Configuration.GetConnectionString("OrleansStorage");
        builder
            .UseOrleans(silo =>
            {
                silo
                    .UseLocalhostClustering()
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "dev";
                        options.ServiceId = "OrleansExample";
                    })
                    .AddAdoNetGrainStorageAsDefault(
                        options =>
                        {
                            options.Invariant = adoNetInvariant;
                            options.ConnectionString = adoNetConnectionString;
                        })
                    .ConfigureLogging(logging => logging.AddConsole());
            });
        
    }
}