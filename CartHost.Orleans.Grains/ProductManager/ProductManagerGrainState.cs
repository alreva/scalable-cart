namespace CartHost.Orleans.Grains.ProductManager;

public record ProductManagerGrainState
{
    public HashSet<(int cartid, int productId)> CartProducts { get; set; } = [];
}