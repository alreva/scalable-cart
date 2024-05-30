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
        
        public record GetProductDetails(int ProductId);
    }
    
    public static class C
    {
        public record UpdateProductPrice(int ProductId, decimal NewPrice);
    }
    
    public record TopCategories(Category[] Categories);
    public record CategoryProducts(Category Category, CatalogItem[] Products, int TotalProducts);
    public record Category(string Name);
    
    public class CatalogItem(
        int id,
        string name,
        string description,
        decimal price,
        string category,
        string brand
    )
    {
        public int Id { get; set; } = id;
        public string Name { get; set; } = name;
        public string Description { get; set; } = description;
        public decimal Price { get; set; } = price;
        public string Category { get; set; } = category;
        public string Brand { get; set; } = brand;

        public void Deconstruct(out int id, out string name, out string description, out decimal price, out string category, out string brand)
        {
            id = this.Id;
            name = this.Name;
            description = this.Description;
            price = this.Price;
            category = this.Category;
            brand = this.Brand;
        }
    }
}