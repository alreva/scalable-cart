namespace CartHost.Marten.Orchestration;

public static class ProductManagementService
{
    public static void RequireProductManagement(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<CartHubNotifier>();
        builder.Services.AddScoped<CartRegistry>();
        builder.Services.AddScoped<ProductManager>();
    }
}