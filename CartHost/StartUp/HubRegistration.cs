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
                    policy.AllowAnyOrigin();
                    policy.AllowAnyMethod();
                    policy.AllowAnyHeader();
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