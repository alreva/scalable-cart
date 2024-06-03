namespace CartHost.Orleans.Grains.ProductManager;

public record ProductGrainState
{
    public decimal? Price { get; set; } = 0M;
}