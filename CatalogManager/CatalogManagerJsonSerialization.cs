using System.Text.Json.Serialization;
using SharedMessages;

namespace CatalogManager;

[JsonSerializable(typeof(ProductManagerMessages.TopCategories))]
[JsonSerializable(typeof(ProductManagerMessages.CategoryProducts))]
[JsonSerializable(typeof(ProductManagerMessages.CatalogItem))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class CatalogManagerJsonSerialization : JsonSerializerContext
{
    
}