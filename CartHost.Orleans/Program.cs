using CartHost.Orleans;
using CartHost.Orleans.Cart;
using CartHost.Orleans.Hello;
using CartHost.Orleans.ProductManager;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureOrleansServer();
builder.AddProducts();
builder.Services.AddCart();
builder.Services.AddSignalR();
builder.Services.ConfigureHubs();

var app = builder.Build();

app.UseCors();
app.MapGet("/", () => "Hello World!");
app.MapCart();
app.MapProducts();
app.MapHello();

app.Run();