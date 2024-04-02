using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.Api.Contexts;
using Order.Api.ViewModels;
using Shared.Messages;
using Shared.OrderEvents;
using Shared.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMq"]);
    });
});

builder.Services.AddDbContext<OrderDbContexts>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.MapPost("/create-order", async (CreateOrderVm model, OrderDbContexts context, ISendEndpointProvider sendEndpointProvider) =>
{
    Order.Api.Models.Order order = new()
    {
        BuyerId = model.BuyerId,
        CreatedDate = DateTime.UtcNow,
        OrderStatus = Order.Api.Enums.OrderStatus.Suspend,
        TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
        OrderItems = model.OrderItems.Select(oi => new Order.Api.Models.OrderItem
        {
            Count = oi.Count,
            Price = oi.Price,
            ProductId = oi.ProductId
        }).ToList()
    };

    await context.Orders.AddAsync(order);
    await context.SaveChangesAsync();

    //Sipari� ba�lat�ld� eventi haz�rlan�yor
    OrderStartedEvent orderStartedEvent = new()
    {
        BuyerId = order.BuyerId,
        OrderId = order.Id,
        TotalPrice = model.OrderItems.Sum(oi=>oi.Count * oi.Price),
        OrderItems = model.OrderItems.Select(oi => new OrderItemMessage
        {
            Count = oi.Count,
            Price = oi.Price,
            ProductId = oi.ProductId
        }).ToList()
    };

    var sendEndPoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqSettings.StateMachineQueue}"));
    await sendEndPoint.Send<OrderStartedEvent>(orderStartedEvent);
    
});
app.Run();
