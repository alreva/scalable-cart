using System.Net.Http.Json;
using Xunit;

namespace CartHost.Tests;

public class TestScenario
{
    private readonly HttpClient _http = new()
    {
        BaseAddress = new Uri("http://localhost:5254")
    };

    [Fact]
    public async Task RunCartOperations()
    {
        await CallGetCart(1);
        await CallGetCart(2);
        await CallGetCart(3);


        await CallAddProduct(1, "product1", 11.11M);
        await CallAddProduct(1, "product2", 22.22M);
        await CallAddProduct(1, "product3", 33.33M);
        await CallAddProduct(2, "product4", 44.44M);
        await CallAddProduct(3, "product2", 22.22M);
        await CallAddProduct(3, "product5", 55.55M);

        await Task.Delay(TimeSpan.FromSeconds(1));
        await CallUpdateProductPrice("product2", 222.22M);
    }
    
    [Fact]
    public async Task RunPriceUpdate()
    {
        await CallUpdateProductPrice("product2", 222.22M);
    }

    private async Task CallAddProduct(int cartId, string productName, decimal price)
    {
        await _http.PostAsJsonAsync<object>($"/cart/{cartId}/add-product", new
        {
            ProductName = productName,
            Price = price
        });
    }

    private async Task CallUpdateProductPrice(string productName, decimal price)
    {
        await _http.PutAsJsonAsync<object>($"/product/{productName}", new
        {
            Price = price
        });
    }

    private async Task<HttpResponseMessage> CallGetCart(int cartId)
    {
        return await _http.GetAsync($"/cart/{cartId}");
    }
}