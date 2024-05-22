using CartHost;
using CartHost.Cart;
using CartHost.ProductManager;
using CartHost.StartUp;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureAkka(builder.Configuration);
builder.Services.ConfigureHubs();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapCart();
app.MapProducts();
app.UseHubs();

HubContext.Cart = app.Services.GetRequiredService<IHubContext<CartHub>>();

app.Run();