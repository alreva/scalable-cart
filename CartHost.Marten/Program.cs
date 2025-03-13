using CartHost.Marten.Cart;
using CartHost.Marten.Catalog;
using CartHost.Marten.Hello;
using CartHost.Marten.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.ConfigureMarten();
builder.AddProducts();
builder.AddCart();
builder.Services.AddSignalR();
builder.Services.ConfigureHubs();

var app = builder.Build();

app.UseCors();
app.MapOpenApi();
app.MapGet("/", () => "Hello World!");
app.MapCart();
app.MapProducts();
app.MapHello();

await app.RunAsync();