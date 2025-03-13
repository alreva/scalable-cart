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
                var productId = R.Next(1, 100);
                await CallAddProduct(i, productId, R.Next(1, 1000));
            }
        }

        int referenceCartId = -1;
        foreach (var i in Enumerable.Range(1, R.Next(1, 20)))
        {
            await CallAddProduct(i, 2, 22.22M);
            referenceCartId = i;
        }
        
        await CallUpdateProductPrice(2, 222.22M);
        await Task.Delay(TimeSpan.FromSeconds(5));
        
        await VerifyCartHasProductPrice(referenceCartId, 2, 222.22M);
        await Task.Delay(TimeSpan.FromSeconds(5));
        
        await CallUpdateProductPrice(2, 22.22M);
        await Task.Delay(TimeSpan.FromSeconds(5));

        await VerifyCartHasProductPrice(referenceCartId, 2, 22.22M);
    }

    [Fact]
    public async Task E2EProductPriceUpdateSmokeCheck()
    {
        await GetCart(1);
        await GetCart(2);
        await GetCart(3);


        await CallAddProduct(5000, 1, 11.11M);
        await CallAddProduct(5000, 2, 22.22M);
        await CallAddProduct(5000, 3, 33.33M);
        await CallAddProduct(2, 4, 44.44M);
        await CallAddProduct(3, 2, 22.22M);
        await CallAddProduct(3, 5, 55.55M);

        await CallUpdateProductPrice(2, 222.22M);
        await Task.Delay(TimeSpan.FromSeconds(5));
        
        await VerifyCartHasProductPrice(5000, 2, 222.22M);
        await Task.Delay(TimeSpan.FromSeconds(5));

        await CallUpdateProductPrice(2, 22.22M);
        await Task.Delay(TimeSpan.FromSeconds(5));
        await VerifyCartHasProductPrice(5000, 2, 22.22M);
    }

    [Fact]
    public async Task PriceUpdateCheck()
    {
        await CallUpdateProductPrice(2, 222.22M);
        await VerifyCartHasProductPrice(1, 2, 222.22M);
        await VerifyCartHasProductPrice(3, 2, 222.22M);
        await CallUpdateProductPrice(2, 22.22M);
        await VerifyCartHasProductPrice(1, 2, 22.22M);
        await VerifyCartHasProductPrice(3, 2, 22.22M);
    }

    private async Task VerifyCartHasProductPrice(int cartId, int productId, decimal price)
    {
        var cart = (await GetCart(cartId)).Details;
        cart.LineItems
            .Find(li => li.ProductId == productId)!
            .Price.Should().Be(price);
    }

    private async Task CallAddProduct(int cartId, int productId, decimal price)
    {
        await _http.PostAsJsonAsync<object>($"/cart/{cartId}/products", new
        {
            ProductId = productId,
            Price = price
        });
    }

    private async Task CallUpdateProductPrice(int productId, decimal price)
    {
        await _http.PutAsJsonAsync<object>($"/product/{productId}", new
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
    private record LineItemDto(int ProductId, decimal Price/*, int Quantity*/);
}
