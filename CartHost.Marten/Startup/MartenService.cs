using Marten;
using Marten.Events;
using Weasel.Core;

namespace CartHost.Marten.Startup;

public static class MartenService
{
    public static void ConfigureMarten(this WebApplicationBuilder builder)
    {
        var martenConfig = builder.Configuration.GetSection("Marten").Get<MartenOptions>()!;

        builder.Services.AddMarten(options =>
        {
            options.Connection(martenConfig.ConnectionString!);
            options.UseSystemTextJsonForSerialization();
            options.Events.StreamIdentity = StreamIdentity.AsString;
            if (builder.Environment.IsDevelopment())
            {
                options.AutoCreateSchemaObjects = AutoCreate.All;
            }
        });
    }
}

public class MartenOptions
{
    public string? ConnectionString { get; set; }
}