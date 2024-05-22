using CartHost.Cart;

namespace CartHost.StartUp;

public static class HubRegistration
{
    public static void ConfigureHubs(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                policy =>
                {
                    policy.WithOrigins("http://localhost:3000");
                    policy.WithMethods("GET", "POST", "PUT", "DELETE");
                    policy.WithHeaders("x-signalr-user-agent", "Content-Type", "Authorization", "Accept", "X-Requested-With", "Origin", "Cache-Control");
                    policy.WithExposedHeaders("Content-Length", "ETag", "Link", "X-Total-Count");
                });
        });
        services.AddSignalR();
    }
    
    public static void UseHubs(this IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseCors();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<CartHub>("/hubs/cart");
        });
    }
}