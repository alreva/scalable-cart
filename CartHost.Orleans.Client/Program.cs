using CartHost.Orleans.Client;
using CartHost.Orleans.Client.Cart;
using CartHost.Orleans.Client.Hello;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureOrleansClient();
builder.Services.AddCart();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapCart();
app.MapHello();

app.Run();