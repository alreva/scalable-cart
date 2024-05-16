using CartHost.Cart;
using CartHost.ProductManager;
using CartHost.StartUp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureAkka(builder.Configuration.GetConnectionString("AkkaPersistence")!);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapCart();
app.MapProducts();

app.Run();