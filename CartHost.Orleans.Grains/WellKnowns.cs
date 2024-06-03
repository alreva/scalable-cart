namespace CartHost.Orleans.Grains;

public static class WellKnownStorageNames
{
    public const string Cart = "scalable-cart-storage";
}

public static class WellKnownGrainIds
{
    public const string CartNotifier = "CartNotifier";
    public const string ProductManager = "ProductManager";
}