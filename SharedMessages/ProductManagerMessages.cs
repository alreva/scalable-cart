namespace SharedMessages;

public static class ProductManagerMessages
{
    public static class Q
    {
        public record GetTopCategories
        {
            public static GetTopCategories Query { get; } = new();
        };
        public record GetCategoryProducts(Category Category, Paging Paging = default)
        {
            public static GetCategoryProducts Query(Category category, Paging paging) => new(category, paging);
        }
    }
    
    public static class C
    {
        public record UpdateProductPrice(string ProductName, decimal NewPrice);
    }
    
    public record TopCategories(Category[] Categories);
    public record CategoryProducts(Category Category, CatalogItem[] Products);
    public record Category(string Name);
    
    public record CatalogItem(
        int Id,
        string Name,
        string Description,
        decimal Price,
        string Category,
        string Brand
    );
}