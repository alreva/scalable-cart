using System.Net;

namespace CatalogManager;

public static class Links
{
    public static string Catalog() => $"/catalog";
    public static string Category(string name) => $"/category/{WebUtility.UrlEncode(name)}";
    public static string Product(int id) => $"/product/{id}";
}