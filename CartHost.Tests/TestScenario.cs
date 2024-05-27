using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace CartHost.Tests;

public class TestScenario
{
    private readonly Random R = Random.Shared;
    
    private readonly HttpClient _http = new()
    {
        BaseAddress = new Uri("http://localhost:5254")
    };

    [Fact]
    public async Task VolumeSmokeCheck()
    {
        foreach (var i in Enumerable.Range(1, 5000))
        {
            await GetCart(i);
            foreach (var _ in Enumerable.Range(1, R.Next(1, 10)))
            {
                var productName = "product" + R.Next(1, 100);
                await CallAddProduct(i, productName, R.Next(1, 1000));
            }
        }

        int referenceCartId = -1;
        foreach (var i in Enumerable.Range(1, R.Next(1, 20)))
        {
            await CallAddProduct(i, "product2", 22.22M);
            referenceCartId = i;
        }
        await Task.Delay(TimeSpan.FromSeconds(1));
        
        await CallUpdateProductPrice("product2", 222.22M);
        await VerifyCartHasProductPrice(referenceCartId, "product2", 222.22M);
        await CallUpdateProductPrice("product2", 22.22M);
        await VerifyCartHasProductPrice(referenceCartId, "product2", 22.22M);
    }

    [Fact]
    public async Task E2EProductPriceUpdateSmokeCheck()
    {
        await GetCart(1);
        await GetCart(2);
        await GetCart(3);


        await CallAddProduct(5000, "product1", 11.11M);
        await CallAddProduct(5000, "product2", 22.22M);
        await CallAddProduct(5000, "product3", 33.33M);
        await CallAddProduct(2, "product4", 44.44M);
        await CallAddProduct(3, "product2", 22.22M);
        await CallAddProduct(3, "product5", 55.55M);

        await CallUpdateProductPrice("product2", 222.22M);
        await Task.Delay(TimeSpan.FromSeconds(5));
        await VerifyCartHasProductPrice(5000, "product2", 222.22M);

        await CallUpdateProductPrice("product2", 22.22M);
        await Task.Delay(TimeSpan.FromSeconds(5));
        await VerifyCartHasProductPrice(5000, "product2", 22.22M);
    }

    [Fact]
    public async Task PriceUpdateCheck()
    {
        await CallUpdateProductPrice("product2", 222.22M);
        await VerifyCartHasProductPrice(1, "product2", 222.22M);
        await VerifyCartHasProductPrice(3, "product2", 222.22M);
        await CallUpdateProductPrice("product2", 22.22M);
        await VerifyCartHasProductPrice(1, "product2", 22.22M);
        await VerifyCartHasProductPrice(3, "product2", 22.22M);
    }

    private async Task VerifyCartHasProductPrice(int cartId, string productName, decimal price)
    {
        var cart = (await GetCart(cartId)).Details;
        cart.LineItems
            .Find(li => li.ProductName == productName)!
            .Price.Should().Be(price);
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

    private async Task<GetCarResponseDto> GetCart(int cartId)
    {
        var r = await _http.GetAsync($"/cart/{cartId}");
        r.EnsureSuccessStatusCode();
        return (await r.Content.ReadFromJsonAsync<GetCarResponseDto>())!;
    }
    
    // ReSharper disable once ClassNeverInstantiated.Local
    private record GetCarResponseDto(/*string Path, */CartDto Details);
    // ReSharper disable once ClassNeverInstantiated.Local
    private record CartDto(/*int CartId, */List<LineItemDto> LineItems);
    // ReSharper disable once ClassNeverInstantiated.Local
    private record LineItemDto(string ProductName, decimal Price/*, int Quantity*/);
}
